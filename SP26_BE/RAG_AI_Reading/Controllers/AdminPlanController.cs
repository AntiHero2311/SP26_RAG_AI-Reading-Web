using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPlanController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public AdminPlanController()
        {
            _subscriptionService = new SubscriptionService();
        }

        /// <summary>
        /// Lấy danh sách toàn bộ các gói cước (bao gồm cả gói đang ẩn)
        /// </summary>
        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans([FromQuery] bool includeInactive = false)
        {
            var (success, message, plans) = await _subscriptionService.GetAllPlansAsync(includeInactive);

            if (!success || plans == null)
            {
                return BadRequest(new { message });
            }

            var planList = plans.Select(p => new SubscriptionPlanResponseDto
            {
                PlanId = p.PlanId,
                PlanName = p.PlanName,
                Price = p.Price ?? 0,
                MaxAnalysisCount = p.MaxAnalysisCount ?? 0,
                MaxTokenLimit = p.MaxTokenLimit ?? 0,
                Description = p.Description,
                IsActive = p.IsActive ?? true,
                TotalSubscribers = p.UserSubscriptions?.Count(s => s.Status == "Active") ?? 0
            }).ToList();

            return Ok(new
            {
                message,
                data = planList,
                count = planList.Count
            });
        }

        /// <summary>
        /// Tạo gói cước mới (Nhập Tên, Giá, Số lượt AI, Giới hạn Token)
        /// </summary>
        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, plan) = await _subscriptionService.CreatePlanAsync(
                request.PlanName,
                request.Price,
                request.MaxAnalysisCount,
                request.MaxTokenLimit,
                request.Description
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new SubscriptionPlanResponseDto
            {
                PlanId = plan!.PlanId,
                PlanName = plan.PlanName,
                Price = plan.Price ?? 0,
                MaxAnalysisCount = plan.MaxAnalysisCount ?? 0,
                MaxTokenLimit = plan.MaxTokenLimit ?? 0,
                Description = plan.Description,
                IsActive = plan.IsActive ?? true,
                TotalSubscribers = 0
            };

            return CreatedAtAction(nameof(GetPlanById), new { id = plan.PlanId }, new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Lấy chi tiết thông tin một gói cước
        /// </summary>
        [HttpGet("plans/{id}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var (success, message, plan) = await _subscriptionService.GetPlanByIdAsync(id);

            if (!success || plan == null)
            {
                return NotFound(new { message });
            }

            var response = new SubscriptionPlanResponseDto
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                Price = plan.Price ?? 0,
                MaxAnalysisCount = plan.MaxAnalysisCount ?? 0,
                MaxTokenLimit = plan.MaxTokenLimit ?? 0,
                Description = plan.Description,
                IsActive = plan.IsActive ?? true,
                TotalSubscribers = plan.UserSubscriptions?.Count(s => s.Status == "Active") ?? 0
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Cập nhật gói cước (Sửa tên, mô tả). Lưu ý: Thường không nên sửa giá trực tiếp nếu đã có người mua
        /// </summary>
        [HttpPut("plans/{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, plan) = await _subscriptionService.UpdatePlanAsync(
                id,
                request.PlanName,
                request.Price,
                request.MaxAnalysisCount,
                (int)request.MaxTokenLimit, // FIX: Cast long to int
                request.Description
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new SubscriptionPlanResponseDto
            {
                PlanId = plan!.PlanId,
                PlanName = plan.PlanName,
                Price = plan.Price ?? 0,
                MaxAnalysisCount = plan.MaxAnalysisCount ?? 0,
                MaxTokenLimit = plan.MaxTokenLimit ?? 0,
                Description = plan.Description,
                IsActive = plan.IsActive ?? true,
                TotalSubscribers = plan.UserSubscriptions?.Count(s => s.Status == "Active") ?? 0
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Kích hoạt / Vô hiệu hóa gói cước (Active/Inactive). Dùng khi muốn ngừng bán gói
        /// </summary>
        [HttpPatch("plans/{id}/status")]
        public async Task<IActionResult> UpdatePlanStatus(int id, [FromBody] bool isActive)
        {
            var (success, message) = await _subscriptionService.UpdatePlanStatusAsync(id, isActive);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Xem danh sách người dùng đang mua gói (Lọc theo Active/Expired)
        /// </summary>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetAllSubscriptions([FromQuery] string? status = null)
        {
            var (success, message, subscriptions) = await _subscriptionService.GetAllSubscriptionsAsync(status);

            if (!success || subscriptions == null)
            {
                return BadRequest(new { message });
            }

            var subscriptionList = subscriptions.Select(s => new UserSubscriptionResponseDto
            {
                SubscriptionId = s.SubscriptionId,
                UserId = s.UserId,
                UserName = s.User?.FullName ?? "",
                UserEmail = s.User?.Email ?? "",
                PlanId = s.PlanId,
                PlanName = s.Plan?.PlanName ?? "",
                Price = s.Plan?.Price ?? 0,
                Status = s.Status ?? "",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                UsedAnalysisCount = s.UsedAnalysisCount ?? 0,
                UsedTokens = s.UsedTokens ?? 0,
                CreatedAt = s.CreatedAt ?? DateTime.MinValue
            }).ToList();

            return Ok(new
            {
                message,
                data = subscriptionList,
                count = subscriptionList.Count
            });
        }
    }
}