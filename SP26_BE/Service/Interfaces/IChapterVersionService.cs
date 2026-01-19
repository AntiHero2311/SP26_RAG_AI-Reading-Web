using Repository.Models;

namespace Service.Interfaces
{
    public interface IChapterVersionService
    {
        Task<(bool Success, string Message, ChapterVersion? Version)> CreateVersionAsync(
            int userId,
            int chapterId,
            string rawContent);

        Task<(bool Success, string Message, ChapterVersion? Version)> GetVersionByIdAsync(int versionId);

        Task<(bool Success, string Message, List<ChapterVersion>? Versions)> GetVersionsByChapterIdAsync(int chapterId);

        Task<(bool Success, string Message, ChapterVersion? Version)> GetActiveVersionAsync(int chapterId);

        Task<(bool Success, string Message, ChapterVersion? Version)> UpdateVersionAsync(
            int versionId,
            int userId,
            string rawContent);

        Task<(bool Success, string Message)> SetActiveVersionAsync(int versionId, int userId);

        Task<(bool Success, string Message)> DeleteVersionAsync(int versionId, int userId);
    }
}
