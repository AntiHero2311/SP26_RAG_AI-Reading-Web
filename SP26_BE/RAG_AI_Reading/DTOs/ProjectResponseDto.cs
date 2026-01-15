namespace RAG_AI_Reading.DTOs
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Statistics
        public int TotalChapters { get; set; }
        public int TotalChatSessions { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }
}