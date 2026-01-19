namespace RAG_AI_Reading.DTOs
{
    public class SystemLogResponseDto
    {
        public int LogId { get; set; }
        public int? ActorId { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }
}
