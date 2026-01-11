namespace RAG_AI_Reading.DTOs
{
    public class SubscriptionPlanResponseDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AnalysisLimit { get; set; }
        public bool CanReadUnlimited { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TotalSubscribers { get; set; }
    }
}