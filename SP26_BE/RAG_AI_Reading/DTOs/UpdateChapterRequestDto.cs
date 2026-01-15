using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class UpdateChapterRequestDto
    {
        [Required(ErrorMessage = "Tiêu đề chương là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        [Range(1, 10000, ErrorMessage = "Số chương phải từ 1 đến 10000")]
        public int? ChapterNo { get; set; }
    }
}