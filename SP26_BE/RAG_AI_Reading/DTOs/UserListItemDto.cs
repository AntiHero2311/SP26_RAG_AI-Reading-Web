namespace RAG_AI_Reading.DTOs
{
    public class UserListItemDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Thống kê thêm
        public int TotalProjects { get; set; }
        public int TotalComments { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }
    }
}