using Repository.Models;

namespace Service.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(
            string fullName,
            string email,
            string password,
            string? avatarUrl = null);

        Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);

        string GenerateJwtToken(User user);

        string GenerateRefreshToken();

        Task<(bool Success, string Message)> SaveRefreshTokenAsync(int userId, string refreshToken);

        Task<(bool Success, string Message, User? User)> ValidateRefreshTokenAsync(string refreshToken);

        Task<(bool Success, string Message)> RevokeRefreshTokenAsync(int userId);

        Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);

        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string resetToken, string newPassword);
    }
}
