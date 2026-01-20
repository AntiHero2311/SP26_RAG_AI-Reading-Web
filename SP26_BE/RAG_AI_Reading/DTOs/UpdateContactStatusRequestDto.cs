using System.ComponentModel.DataAnnotations;

namespace RAG_AI_Reading.DTOs
{
    public class UpdateContactStatusRequestDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // "Active", "Closed", "Pending"
    }
}
