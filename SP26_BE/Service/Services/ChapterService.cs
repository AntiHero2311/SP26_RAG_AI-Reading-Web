using Repository;
using Repository.Models;
using Service.Helpers; // Import SecurityHelper

namespace Service
{
    public class ChapterService
    {
        private readonly ChapterRepository _chapterRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly UserRepository _userRepository;
        public ChapterService(
            ChapterRepository chapterRepo,
            ProjectRepository projectRepo,
            UserRepository userRepo)
        {
            _chapterRepository = chapterRepo;
            _projectRepository = projectRepo;
            _userRepository = userRepo;
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> CreateChapterAsync(
            int userId,
            int projectId,
            int chapterNo,
            string title,
            string? summary)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy dự án truyện", null);

            if (project.AuthorId != userId) return (false, "Bạn không có quyền thêm chương", null);

            var author = await _userRepository.GetByIdAsync(userId);
            if (string.IsNullOrEmpty(author?.DataEncryptionKey))
                return (false, "Lỗi: Tài khoản chưa có khóa mã hóa", null);

            string encryptionKey = author.DataEncryptionKey;

            if (await _chapterRepository.ExistsAsync(projectId, chapterNo))
                return (false, $"Chương số {chapterNo} đã tồn tại", null);

            if (string.IsNullOrWhiteSpace(title)) return (false, "Tiêu đề chương là bắt buộc", null);

            var newChapter = new Chapter
            {
                ProjectId = projectId,
                ChapterNo = chapterNo,
                Title = SecurityHelper.Encrypt(title.Trim(), encryptionKey),
                Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), encryptionKey) : null,
                CreatedAt = DateTime.UtcNow
            };

            var createdChapter = await _chapterRepository.CreateAsync(newChapter);

            createdChapter.Title = title;
            createdChapter.Summary = summary;
            createdChapter.Project = project;

            return (true, "Tạo chương thành công", createdChapter);
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> GetChapterByIdAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return (false, "Không tìm thấy chương", null);

            var project = await _projectRepository.GetByIdAsync(chapter.ProjectId);
            var author = await _userRepository.GetByIdAsync(project.AuthorId);

            chapter.Title = SecurityHelper.Decrypt(chapter.Title, author.DataEncryptionKey);
            chapter.Summary = SecurityHelper.Decrypt(chapter.Summary, author.DataEncryptionKey);

            return (true, "Thành công", chapter);
        }

        public async Task<(bool Success, string Message, List<Chapter>? Chapters)> GetChaptersByProjectIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy dự án truyện", null);

            var author = await _userRepository.GetByIdAsync(project.AuthorId);

            var chapters = await _chapterRepository.GetByProjectIdAsync(projectId);

            foreach (var c in chapters)
            {
                c.Title = SecurityHelper.Decrypt(c.Title, author.DataEncryptionKey);
                c.Summary = SecurityHelper.Decrypt(c.Summary, author.DataEncryptionKey);
            }

            return (true, "Lấy danh sách chương thành công", chapters);
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> UpdateChapterAsync(
            int userId,
            int chapterId,
            string title,
            string? summary,
            int? chapterNo)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return (false, "Không tìm thấy chương", null);

            var project = await _projectRepository.GetByIdAsync(chapter.ProjectId);
            if (project.AuthorId != userId) return (false, "Không có quyền chỉnh sửa", null);

            var author = await _userRepository.GetByIdAsync(userId);

            if (string.IsNullOrWhiteSpace(title)) return (false, "Tiêu đề bắt buộc", null);

            if (chapterNo.HasValue && chapterNo.Value != chapter.ChapterNo)
            {
                if (await _chapterRepository.ExistsAsync(chapter.ProjectId, chapterNo.Value))
                    return (false, $"Chương số {chapterNo.Value} đã tồn tại", null);
                chapter.ChapterNo = chapterNo.Value;
            }

            chapter.Title = SecurityHelper.Encrypt(title.Trim(), author.DataEncryptionKey);
            chapter.Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), author.DataEncryptionKey) : null;
            chapter.UpdatedAt = DateTime.UtcNow;

            var updatedChapter = await _chapterRepository.UpdateAsync(chapter);

            updatedChapter.Title = title;
            updatedChapter.Summary = summary;

            return (true, "Cập nhật chương thành công", updatedChapter);
        }

        public async Task<(bool Success, string Message)> DeleteChapterAsync(int userId, int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return (false, "Không tìm thấy chương");

            var project = await _projectRepository.GetByIdAsync(chapter.ProjectId);
            if (project.AuthorId != userId) return (false, "Không có quyền xóa");

            var success = await _chapterRepository.DeleteAsync(chapterId);
            return success ? (true, "Xóa chương thành công") : (false, "Xóa thất bại");
        }

        public async Task<(bool Success, string Message, int NextChapterNo)> GetNextChapterNoAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy dự án", 0);

            var maxChapterNo = await _chapterRepository.GetMaxChapterNoAsync(projectId);
            return (true, "Thành công", maxChapterNo + 1);
        }
    }
}