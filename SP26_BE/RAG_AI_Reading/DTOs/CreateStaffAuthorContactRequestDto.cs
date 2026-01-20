using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class CreateStaffAuthorContactRequestDto
    {
        [Required]
        public int StaffId { get; set; }

        [Required]
        public int AuthorId { get; set; }
    }
}
