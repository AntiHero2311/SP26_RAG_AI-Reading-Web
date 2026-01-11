using Repository;
using Repository.Models;

namespace Service
{
    public class SubscriptionService
    {
        private readonly SubscriptionPlanRepository _planRepository;
        private readonly UserSubscriptionRepository _subscriptionRepository;

        public SubscriptionService()
        {
            _planRepository = new SubscriptionPlanRepository();
            _subscriptionRepository = new UserSubscriptionRepository();
        }

        // Plans Management
        public async Task<(bool Success, string Message, List<SubscriptionPlan>? Plans)> GetAllPlansAsync(
            bool includeInactive = false)
        {
            var plans = await _planRepository.GetAllPlansAsync(includeInactive);
            return (true, "Lấy danh sách gói cước thành công", plans);
        }

        public async Task<(bool Success, string Message, SubscriptionPlan? Plan)> GetPlanByIdAsync(int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                return (false, "Không tìm thấy gói cước", null);
            }
            return (true, "Lấy thông tin gói cước thành công", plan);
        }

        public async Task<(bool Success, string Message, SubscriptionPlan? Plan)> CreatePlanAsync(
            string planName,
            decimal price,
            int analysisLimit,
            bool canReadUnlimited,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(planName))
            {
                return (false, "Tên gói cước là bắt buộc", null);
            }

            if (price < 0)
            {
                return (false, "Giá phải lớn hơn hoặc bằng 0", null);
            }

            var newPlan = new SubscriptionPlan
            {
                PlanName = planName.Trim(),
                Price = price,
                AnalysisLimit = analysisLimit,
                CanReadUnlimited = canReadUnlimited,
                Description = description?.Trim(),
                IsActive = true
            };

            var createdPlan = await _planRepository.CreateAsync(newPlan);
            return (true, "Tạo gói cước thành công", createdPlan);
        }

        public async Task<(bool Success, string Message, SubscriptionPlan? Plan)> UpdatePlanAsync(
            int planId,
            string planName,
            decimal price,
            int analysisLimit,
            bool canReadUnlimited,
            string? description)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                return (false, "Không tìm thấy gói cước", null);
            }

            if (string.IsNullOrWhiteSpace(planName))
            {
                return (false, "Tên gói cước là bắt buộc", null);
            }

            if (price < 0)
            {
                return (false, "Giá phải lớn hơn hoặc bằng 0", null);
            }

            plan.PlanName = planName.Trim();
            plan.Price = price;
            plan.AnalysisLimit = analysisLimit;
            plan.CanReadUnlimited = canReadUnlimited;
            plan.Description = description?.Trim();

            var updatedPlan = await _planRepository.UpdateAsync(plan);
            return (true, "Cập nhật gói cước thành công", updatedPlan);
        }

        public async Task<(bool Success, string Message)> UpdatePlanStatusAsync(int planId, bool isActive)
        {
            var success = await _planRepository.UpdateStatusAsync(planId, isActive);
            if (!success)
            {
                return (false, "Không tìm thấy gói cước");
            }

            var statusText = isActive ? "kích hoạt" : "vô hiệu hóa";
            return (true, $"Đã {statusText} gói cước thành công");
        }

        // User Subscriptions
        public async Task<(bool Success, string Message, List<UserSubscription>? Subscriptions)> GetAllSubscriptionsAsync(
            string? status = null)
        {
            var subscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync(status);
            return (true, "Lấy danh sách người dùng đang mua gói thành công", subscriptions);
        }
    }
}