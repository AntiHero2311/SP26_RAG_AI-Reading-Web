using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Repository.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Service
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _userRepository = new UserRepository();
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            string fullName,
            string email,
            string password,
            string? avatarUrl = null)
        {
            if (await _userRepository.EmailExistsAsync(email))
            {
                return (false, "Email đã được sử dụng", null);
            }

            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
            {
                return (false, "Tên phải có ít nhất 2 ký tự", null);
            }

            if (!IsValidEmail(email))
            {
                return (false, "Email không hợp lệ", null);
            }

            if (password.Length < 6)
            {
                return (false, "Mật khẩu phải có ít nhất 6 ký tự", null);
            }

            string passwordHash = HashPassword(password);

            var newUser = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = passwordHash,
                AvatarUrl = avatarUrl?.Trim() ?? "",
                Role = "Author", // ✅ Đảm bảo Role luôn có giá trị
                IsActive = true,
                CreatedAt = DateTime.UtcNow, // ✅ Dùng UTC
                RefreshToken = "", // ✅ Khởi tạo empty string
                PasswordResetToken = "" // ✅ Khởi tạo empty string
            };

            var createdUser = await _userRepository.CreateAsync(newUser);
            return (true, "Đăng ký thành công", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return (false, "Email hoặc mật khẩu không đúng", null);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Email hoặc mật khẩu không đúng", null);
            }

            if (user.IsActive != true)
            {
                return (false, "Tài khoản đã bị vô hiệu hóa", null);
            }

            return (true, "Đăng nhập thành công", user);
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
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
                expires: DateTime.Now.AddHours(1), // Access token có hạn ngắn
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
            {
                return (false, "Refresh token không hợp lệ", null);
            }

            if (user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return (false, "Refresh token đã hết hạn", null);
            }

            return (true, "Refresh token thành công", user);
        }

        public async Task<(bool Success, string Message)> SaveRefreshTokenAsync(User user, string refreshToken)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // Refresh token có hạn 7 ngày

            await _userRepository.UpdateAsync(user);
            return (true, "Lưu refresh token thành công");
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // Vẫn trả về success để tránh lộ thông tin user có tồn tại hay không
                return (true, "Nếu email tồn tại, link reset mật khẩu đã được gửi");
            }

            // Generate reset token
            var resetToken = GeneratePasswordResetToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiryTime = DateTime.Now.AddHours(1); // Token có hạn 1 giờ

            await _userRepository.UpdateAsync(user);

            // TODO: Gửi email với link reset
            // Ví dụ: https://yourapp.com/reset-password?token={resetToken}
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return (true, $"Token reset mật khẩu: {resetToken} (Trong production, gửi qua email)");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(token);

            if (user == null)
            {
                return (false, "Token không hợp lệ hoặc đã hết hạn");
            }

            if (newPassword.Length < 6)
            {
                return (false, "Mật khẩu phải có ít nhất 6 ký tự");
            }

            // Update password
            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            await _userRepository.UpdateAsync(user);

            return (true, "Đặt lại mật khẩu thành công");
        }

        private string GeneratePasswordResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
