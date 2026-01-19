using Repository;
using Repository.Models;

namespace Service
{
    public class AdminService
    {
        private readonly UserRepository _userRepository;

        public AdminService()
        {
            _userRepository = new UserRepository();
        }

        public async Task<(bool Success, string Message, List<User>? Users, int TotalCount, int TotalPages)> GetAllUsersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? role = null,
            bool includeInactive = false)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 10;

            var (users, totalCount) = await _userRepository.GetAllUsersPaginatedAsync(
                pageNumber, 
                pageSize, 
                searchTerm, 
                role, 
                includeInactive);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return (true, "Lấy danh sách người dùng thành công", users, totalCount, totalPages);
        }
    }
}