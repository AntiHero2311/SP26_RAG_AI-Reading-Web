using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class UpdateProjectRequestDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Thể loại không được vượt quá 50 ký tự")]
        public string? Genre { get; set; }

        public string? Summary { get; set; }

        [MaxLength(500, ErrorMessage = "URL ảnh bìa không được vượt quá 500 ký tự")]
        public string? CoverImageUrl { get; set; }

        public string? Status { get; set; } // Draft, Published, Completed
    }
}