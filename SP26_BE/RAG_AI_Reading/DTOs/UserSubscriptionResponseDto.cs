namespace RAG_AI_Reading.DTOs
{
    public class UserSubscriptionResponseDto
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsedAnalysisCount { get; set; }
        public long UsedTokens { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}