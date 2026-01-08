using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "URL avatar không được vượt quá 500 ký tự")]
        public string? AvatarUrl { get; set; }
    }
}