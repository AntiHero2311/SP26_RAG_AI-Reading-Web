namespace RAG_AI_Reading.DTOs
{
    public class SubscriptionPlanResponseDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxAnalysisCount { get; set; }
        public long MaxTokenLimit { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TotalSubscribers { get; set; }
    }
}