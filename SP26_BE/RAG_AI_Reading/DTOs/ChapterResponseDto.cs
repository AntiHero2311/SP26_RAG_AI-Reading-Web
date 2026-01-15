namespace RAG_AI_Reading.DTOs
{
    public class ChapterResponseDto
    {
        public int ChapterId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int ChapterNo { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalVersions { get; set; }
    }
}