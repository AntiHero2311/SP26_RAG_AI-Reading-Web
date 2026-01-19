using Repository.Models;

namespace Service.Interfaces
{
    public interface ISubscriptionService
    {
        // Plans Management
        Task<(bool Success, string Message, List<SubscriptionPlan>? Plans)> GetAllPlansAsync(
            bool includeInactive = false);

        Task<(bool Success, string Message, SubscriptionPlan? Plan)> GetPlanByIdAsync(int planId);

        Task<(bool Success, string Message, SubscriptionPlan? Plan)> CreatePlanAsync(
            string planName,
            decimal price,
            int analysisLimit,
            long tokenLimit,
            string? description);

        Task<(bool Success, string Message, SubscriptionPlan? Plan)> UpdatePlanAsync(
            int planId,
            string planName,
            decimal price,
            int analysisLimit,
            int tokenLimit,
            string? description);

        Task<(bool Success, string Message)> UpdatePlanStatusAsync(int planId, bool isActive);

        // User Subscriptions
        Task<(bool Success, string Message, UserSubscription? Subscription)> GetUserSubscriptionAsync(int userId);

        Task<(bool Success, string Message, UserSubscription? Subscription)> SubscribeToPlanAsync(
            int userId,
            int planId);
    }
}
