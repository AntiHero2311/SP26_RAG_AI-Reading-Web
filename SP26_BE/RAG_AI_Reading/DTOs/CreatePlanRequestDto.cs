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

        [Range(0, 1000, ErrorMessage = "Số lượt phân tích tối đa phải từ 0 đến 1000")]
        public int MaxAnalysisCount { get; set; } = 10;

        [Range(0, long.MaxValue, ErrorMessage = "Giới hạn token phải lớn hơn 0")]
        public long MaxTokenLimit { get; set; } = 50000;

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
    }
}