using Repository.Models;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string Message, User? User)> GetUserProfileAsync(int userId);

        Task<(bool Success, string Message, User? User)> UpdateUserProfileAsync(
            int userId,
            string fullName,
            string? avatarUrl);
    }
}
