using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class SystemLogRepository
    {
        private readonly StoryAI_DBContext _context;

        public SystemLogRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public SystemLogRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<SystemLog> CreateAsync(SystemLog log)
        {
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();

            await _context.Entry(log)
                .Reference(l => l.Actor)
                .LoadAsync();

            return log;
        }

        public async Task<SystemLog?> GetByIdAsync(int logId)
        {
            return await _context.SystemLogs
                .Include(l => l.Actor)
                .AsTracking()
                .FirstOrDefaultAsync(l => l.LogId == logId);
        }

        public async Task<List<SystemLog>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _context.SystemLogs
                .Include(l => l.Actor)
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<SystemLog> Logs, int TotalCount)> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string? actionType = null,
            int? actorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.SystemLogs
                .Include(l => l.Actor)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                query = query.Where(l => l.ActionType.Contains(actionType));
            }

            if (actorId.HasValue)
            {
                query = query.Where(l => l.ActorId == actorId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= endDate);
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<List<SystemLog>> GetByActionTypeAsync(string actionType, int limit = 100)
        {
            return await _context.SystemLogs
                .Include(l => l.Actor)
                .Where(l => l.ActionType == actionType)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SystemLog>> GetByActorIdAsync(int actorId, int limit = 100)
        {
            return await _context.SystemLogs
                .Include(l => l.Actor)
                .Where(l => l.ActorId == actorId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.SystemLogs.CountAsync();
        }

        public async Task<bool> DeleteAsync(int logId)
        {
            var log = await _context.SystemLogs.FindAsync(logId);
            if (log == null) return false;

            _context.SystemLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var logsToDelete = await _context.SystemLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();

            if (logsToDelete.Count == 0) return false;

            _context.SystemLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
