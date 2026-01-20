using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class CreateStaffAuthorMessageRequestDto
    {
        [Required]
        public int ContactId { get; set; }

        [Required]
        public string MessageText { get; set; }
    }
}
