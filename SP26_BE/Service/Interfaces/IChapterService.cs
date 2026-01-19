using Repository.Models;

namespace Service.Interfaces
{
    public interface IChapterService
    {
        Task<(bool Success, string Message, Chapter? Chapter)> CreateChapterAsync(
            int userId,
            int projectId,
            int chapterNo,
            string title,
            string? summary);

        Task<(bool Success, string Message, Chapter? Chapter)> GetChapterByIdAsync(int chapterId);

        Task<(bool Success, string Message, List<Chapter>? Chapters)> GetChaptersByProjectIdAsync(int projectId);

        Task<(bool Success, string Message, Chapter? Chapter)> UpdateChapterAsync(
            int chapterId,
            int userId,
            string title,
            string? summary);

        Task<(bool Success, string Message)> DeleteChapterAsync(int chapterId, int userId);
    }
}
