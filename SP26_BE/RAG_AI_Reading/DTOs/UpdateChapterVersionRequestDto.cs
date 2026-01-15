using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class UpdateChapterVersionRequestDto
    {
        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string RawContent { get; set; } = string.Empty;
    }
}