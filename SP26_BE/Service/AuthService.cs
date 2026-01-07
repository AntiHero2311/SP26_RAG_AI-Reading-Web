using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Repository.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            // Kiểm tra email đã tồn tại chưa
            if (await _userRepository.EmailExistsAsync(email))
            {
                return (false, "Email đã được sử dụng", null);
            }

            // Validate input
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

            // Hash password
            string passwordHash = HashPassword(password);

            // Tạo user mới
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                AvatarUrl = avatarUrl,
                Role = "User", // Mặc định là User
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createdUser = await _userRepository.CreateUserAsync(newUser);
            return (true, "Đăng ký thành công", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
        {
            // Tìm user theo email
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                return (false, "Email hoặc mật khẩu không đúng", null);
            }

            // Verify password
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
                expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"] ?? "24")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private bool VerifyPassword(string password, string passwordHash)
        {
            // Assuming passwordHash is a Base64-encoded SHA256 hash
            using (var sha256 = SHA256.Create())
            {
                var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var computedHashString = Convert.ToBase64String(computedHash);
                return computedHashString == passwordHash;
            }
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
        private string HashPassword(string password)
        {
            // Hash the password using SHA256 and return as Base64 string
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
