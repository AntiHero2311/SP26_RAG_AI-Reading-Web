namespace RAG_AI_Reading.DTOs
{
    public class ChapterVersionResponseDto
    {
        public int VersionId { get; set; }
        public int ChapterId { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public string RawContent { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalAIJobs { get; set; }
        public int TotalAnalysisReports { get; set; }
        public int TotalChunks { get; set; }
    }
}