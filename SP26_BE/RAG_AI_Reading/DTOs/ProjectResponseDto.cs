namespace RAG_AI_Reading.DTOs
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Summary { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Statistics
        public int TotalVersions { get; set; }
        public int TotalComments { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
    }
}