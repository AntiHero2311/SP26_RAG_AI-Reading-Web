namespace RAG_AI_Reading.DTOs
{
    public class StaffAuthorMessageResponseDto
    {
        public int MessageId { get; set; }
        public int? ContactId { get; set; }
        public string? SenderType { get; set; }
        public int? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }
        public string? MessageText { get; set; }
        public DateTime? SendAt { get; set; }
        public bool? IsRead { get; set; }
    }
}
