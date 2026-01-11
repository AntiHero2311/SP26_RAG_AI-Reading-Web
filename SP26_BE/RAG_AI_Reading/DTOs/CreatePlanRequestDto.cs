using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class CreatePlanRequestDto
    {
        [Required(ErrorMessage = "Tên gói cước là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên gói cước không được vượt quá 50 ký tự")]
        public string PlanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, 999999999, ErrorMessage = "Giá phải từ 0 đến 999,999,999")]
        public decimal Price { get; set; }

        [Range(0, 1000, ErrorMessage = "Số lượt phân tích phải từ 0 đến 1000")]
        public int AnalysisLimit { get; set; } = 5;

        public bool CanReadUnlimited { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        public string? Features { get; set; } // VIP features (quyền đọc, AI analysis limit, etc.)
    }
}