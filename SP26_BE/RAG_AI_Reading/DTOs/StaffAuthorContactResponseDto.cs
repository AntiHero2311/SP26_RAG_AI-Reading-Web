namespace RAG_AI_Reading.DTOs
{
    public class StaffAuthorContactResponseDto
    {
        public int ContactId { get; set; }
        public int? StaffId { get; set; }
        public int? AuthorId { get; set; }
        public string? StaffName { get; set; }
        public string? AuthorName { get; set; }
        public string? StaffAvatar { get; set; }
        public string? AuthorAvatar { get; set; }
        public DateTime? ContactDate { get; set; }
        public string? Status { get; set; }
        public int UnreadCount { get; set; }
        public StaffAuthorMessageResponseDto? LastMessage { get; set; }
        public List<StaffAuthorMessageResponseDto>? Messages { get; set; }
    }
}
