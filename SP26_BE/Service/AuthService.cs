using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Repository.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Service
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        // 1. SỬA: Tiêm UserRepository qua DI thay vì "new" trực tiếp
        public AuthService(IConfiguration configuration, UserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            string fullName,
            string email,
            string password,
            string? avatarUrl = null)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
                return (false, "Tên phải có ít nhất 2 ký tự", null);

            if (!IsValidEmail(email))
                return (false, "Email không hợp lệ", null);

            if (password.Length < 6)
                return (false, "Mật khẩu phải có ít nhất 6 ký tự", null);

            if (await _userRepository.EmailExistsAsync(email))
                return (false, "Email đã được sử dụng", null);

            // 2. SỬA: Hash mật khẩu an toàn (Salt + Hash)
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Convert sang chuỗi base64 gộp để lưu vào 1 cột PasswordHash trong DB
            string storedPassword = $"{Convert.ToBase64String(passwordSalt)}.{Convert.ToBase64String(passwordHash)}";

            var newUser = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = storedPassword, // Lưu chuỗi đã gộp
                AvatarUrl = avatarUrl?.Trim() ?? "",
                Role = "Author",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "",
                PasswordResetToken = "",

                // 3. QUAN TRỌNG: Sinh khóa mã hóa dữ liệu cho User (Project Encryption)
                DataEncryptionKey = Guid.NewGuid().ToString("N")
            };

            var createdUser = await _userRepository.CreateAsync(newUser);
            return (true, "Đăng ký thành công", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                return (false, "Email hoặc mật khẩu không đúng", null);

            if (user.IsActive != true)
                return (false, "Tài khoản đã bị vô hiệu hóa", null);

            // 4. SỬA: Verify mật khẩu theo chuẩn mới
            if (!VerifyPasswordHash(password, user.PasswordHash))
                return (false, "Email hoặc mật khẩu không đúng", null);

            return (true, "Đăng nhập thành công", user);
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Jwt:Key is missing in appsettings");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Dùng UTC
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<(bool Success, string Message, User? User)> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

            if (user == null)
                return (false, "Refresh token không hợp lệ", null);

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow) // Dùng UTC
                return (false, "Refresh token đã hết hạn", null);

            return (true, "Refresh token thành công", user);
        }

        public async Task<(bool Success, string Message)> SaveRefreshTokenAsync(User user, string refreshToken)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);
            return (true, "Lưu refresh token thành công");
        }

        /* ==========================================================
           HELPERS: Password Hashing & Validation (Secure HMACSHA512)
           ========================================================== */

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; // Random Salt tự sinh
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, string storedPassword)
        {
            try
            {
                // Tách chuỗi stored: "Salt.Hash"
                var parts = storedPassword.Split('.');
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var hash = Convert.FromBase64String(parts[1]);

                using (var hmac = new HMACSHA512(salt))
                {
                    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return computedHash.SequenceEqual(hash);
                }
            }
            catch
            {
                return false; // Lỗi format
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Regex đơn giản nhưng hiệu quả hơn try-catch
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch { return false; }
        }

        // Phần Forgot Password giữ nguyên logic cũ nhưng đổi DateTime.Now -> UtcNow
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return (true, "Nếu email tồn tại, link reset mật khẩu đã được gửi");

            var resetToken = GenerateRefreshToken(); // Tái sử dụng hàm sinh random
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateAsync(user);
            return (true, $"Token: {resetToken}");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(token);
            if (user == null) return (false, "Token không hợp lệ hoặc đã hết hạn");

            if (user.PasswordResetTokenExpiryTime < DateTime.UtcNow)
                return (false, "Token đã hết hạn");

            // Tạo hash mới
            CreatePasswordHash(newPassword, out byte[] newHash, out byte[] newSalt);
            string storedPassword = $"{Convert.ToBase64String(newSalt)}.{Convert.ToBase64String(newHash)}";

            user.PasswordHash = storedPassword;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            await _userRepository.UpdateAsync(user);
            return (true, "Đặt lại mật khẩu thành công");
        }
    }
}