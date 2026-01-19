using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class UserSubscriptionRepository
    {
        private readonly StoryAI_DBContext _context;

        public UserSubscriptionRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public UserSubscriptionRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<List<UserSubscription>> GetAllSubscriptionsAsync(string? status = null)
        {
            var query = _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserSubscription?> GetActiveSubscriptionByUserIdAsync(int userId)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && s.Status == "Active")
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }
    }
}