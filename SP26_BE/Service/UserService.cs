using Repository;
using Repository.Models;

namespace Service
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService()
        {
            _userRepository = new UserRepository();
        }

        public async Task<(bool Success, string Message, User? User)> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetProfileByIdAsync(userId);

            if (user == null)
            {
                return (false, "Không tìm thấy người dùng", null);
            }

            return (true, "Lấy thông tin thành công", user);
        }

        public async Task<(bool Success, string Message, User? User)> UpdateUserProfileAsync(
            int userId, 
            string fullName, 
            string? avatarUrl)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2)
            {
                return (false, "Tên phải có ít nhất 2 ký tự", null);
            }

            if (fullName.Length > 100)
            {
                return (false, "Tên không được vượt quá 100 ký tự", null);
            }

            // Update profile
            var success = await _userRepository.UpdateProfileAsync(userId, fullName, avatarUrl);

            if (!success)
            {
                return (false, "Không tìm thấy người dùng", null);
            }

            // Get updated user
            var user = await _userRepository.GetProfileByIdAsync(userId);

            return (true, "Cập nhật thông tin thành công", user);
        }
    }
}