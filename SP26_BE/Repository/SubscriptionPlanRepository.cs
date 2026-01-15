using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class SubscriptionPlanRepository
    {
        private readonly StoryAI_DBContext _context;

        public SubscriptionPlanRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public SubscriptionPlanRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync(bool includeInactive = false)
        {
            var query = _context.SubscriptionPlans
                .Include(p => p.UserSubscriptions)
                .AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive == true);
            }

            return await query
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(int planId)
        {
            return await _context.SubscriptionPlans
                .Include(p => p.UserSubscriptions)
                .AsTracking()
                .FirstOrDefaultAsync(p => p.PlanId == planId);
        }

        public async Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan)
        {
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan)
        {
            var existingEntry = _context.ChangeTracker.Entries<SubscriptionPlan>()
                .FirstOrDefault(e => e.Entity.PlanId == plan.PlanId);
            
            if (existingEntry != null)
            {
                existingEntry.State = EntityState.Detached;
            }

            _context.SubscriptionPlans.Update(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> UpdateStatusAsync(int planId, bool isActive)
        {
            var plan = await GetByIdAsync(planId);
            if (plan == null)
                return false;

            plan.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int planId)
        {
            return await _context.SubscriptionPlans.AnyAsync(p => p.PlanId == planId);
        }
    }
}