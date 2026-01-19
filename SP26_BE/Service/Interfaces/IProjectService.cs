using Repository.Models;

namespace Service.Interfaces
{
    public interface IProjectService
    {
        Task<(bool Success, string Message, Project? Project)> CreateProjectAsync(
            int authorId,
            string title,
            string? summary,
            string? coverImageUrl);

        Task<(bool Success, string Message, List<Project>? Projects)> GetMyProjectsAsync(
            int authorId,
            bool includeDraft = true);

        Task<(bool Success, string Message, Project? Project)> GetProjectByIdAsync(int projectId);

        Task<(bool Success, string Message, Project? Project)> UpdateProjectAsync(
            int projectId,
            int userId,
            string title,
            string? summary,
            string? coverImageUrl);

        Task<(bool Success, string Message)> DeleteProjectAsync(int projectId, int userId);
    }
}
