using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class CreateChapterVersionRequestDto
    {
        [Required(ErrorMessage = "Chapter ID là bắt buộc")]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string RawContent { get; set; } = string.Empty;
    }
}