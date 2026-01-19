using Repository;
using Repository.Models;

namespace Service
{
    public class SystemLogService
    {
        private readonly SystemLogRepository _logRepository;
        private readonly UserRepository _userRepository;

        public SystemLogService(SystemLogRepository logRepository, UserRepository userRepository)
        {
            _logRepository = logRepository;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, SystemLog? Log)> CreateLogAsync(
            int? actorId,
            string actionType,
            string description)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                return (false, "Loại hành động là bắt buộc", null);

            if (string.IsNullOrWhiteSpace(description))
                return (false, "Mô tả là bắt buộc", null);

            var newLog = new SystemLog
            {
                ActorId = actorId,
                ActionType = actionType.Trim(),
                Description = description.Trim(),
                Timestamp = DateTime.UtcNow
            };

            var createdLog = await _logRepository.CreateAsync(newLog);
            return (true, "Ghi nhật ký thành công", createdLog);
        }

        public async Task<(bool Success, string Message, SystemLog? Log)> GetLogByIdAsync(int logId)
        {
            var log = await _logRepository.GetByIdAsync(logId);
            if (log == null) return (false, "Không tìm thấy nhật ký", null);

            return (true, "Thành công", log);
        }

        public async Task<(bool Success, string Message, List<SystemLog>? Logs, int TotalCount, int TotalPages)> GetLogsAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string? actionType = null,
            int? actorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var (logs, totalCount) = await _logRepository.GetPaginatedAsync(
                pageNumber,
                pageSize,
                actionType,
                actorId,
                startDate,
                endDate);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return (true, "Lấy danh sách nhật ký thành công", logs, totalCount, totalPages);
        }

        public async Task<(bool Success, string Message, List<SystemLog>? Logs)> GetLogsByActionTypeAsync(
            string actionType,
            int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                return (false, "Loại hành động không được để trống", null);

            var logs = await _logRepository.GetByActionTypeAsync(actionType, limit);
            return (true, "Thành công", logs);
        }

        public async Task<(bool Success, string Message, List<SystemLog>? Logs)> GetLogsByActorAsync(
            int actorId,
            int limit = 100)
        {
            var actor = await _userRepository.GetByIdAsync(actorId);
            if (actor == null) return (false, "Người dùng không tồn tại", null);

            var logs = await _logRepository.GetByActorIdAsync(actorId, limit);
            return (true, "Thành công", logs);
        }

        public async Task<(bool Success, string Message)> DeleteLogAsync(int logId)
        {
            var success = await _logRepository.DeleteAsync(logId);
            if (!success) return (false, "Không tìm thấy nhật ký");

            return (true, "Xóa nhật ký thành công");
        }

        public async Task<(bool Success, string Message)> DeleteOldLogsAsync(int daysOld)
        {
            if (daysOld < 1) return (false, "Số ngày phải lớn hơn 0");

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var success = await _logRepository.DeleteOlderThanAsync(cutoffDate);

            if (!success) return (false, "Không có nhật ký nào để xóa");

            return (true, $"Xóa nhật ký cũ hơn {daysOld} ngày thành công");
        }

        public async Task<(bool Success, string Message, int TotalLogs)> GetTotalCountAsync()
        {
            var totalCount = await _logRepository.GetTotalCountAsync();
            return (true, "Thành công", totalCount);
        }
    }
}
