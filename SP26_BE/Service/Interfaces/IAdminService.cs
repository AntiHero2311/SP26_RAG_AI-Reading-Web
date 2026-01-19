using Repository.Models;

namespace Service.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, string Message, List<User>? Users, int TotalCount, int TotalPages)> GetAllUsersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? role = null,
            bool includeInactive = false);
    }
}
