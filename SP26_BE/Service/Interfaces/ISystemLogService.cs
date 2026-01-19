using Repository.Models;

namespace Service.Interfaces
{
    public interface ISystemLogService
    {
        Task<(bool Success, string Message, SystemLog? Log)> CreateLogAsync(
            int? actorId,
            string actionType,
            string description);

        Task<(bool Success, string Message, SystemLog? Log)> GetLogByIdAsync(int logId);

        Task<(bool Success, string Message, List<SystemLog>? Logs, int TotalCount, int TotalPages)> GetLogsAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string? actionType = null,
            int? actorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<(bool Success, string Message, List<SystemLog>? Logs)> GetLogsByActionTypeAsync(
            string actionType,
            int limit = 100);

        Task<(bool Success, string Message, List<SystemLog>? Logs)> GetLogsByActorAsync(
            int actorId,
            int limit = 100);

        Task<(bool Success, string Message)> DeleteLogAsync(int logId);

        Task<(bool Success, string Message)> DeleteOldLogsAsync(int daysOld);

        Task<(bool Success, string Message, int TotalLogs)> GetTotalCountAsync();
    }
}
