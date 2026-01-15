using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class CreateProjectRequestDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        [MaxLength(500, ErrorMessage = "URL ảnh bìa không được vượt quá 500 ký tự")]
        public string? CoverImageUrl { get; set; }
    }
}