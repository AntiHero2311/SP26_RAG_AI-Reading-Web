using Repository;
using Repository.Models;

namespace Service
{
    public class ChapterService
    {
        private readonly ChapterRepository _chapterRepository;
        private readonly ProjectRepository _projectRepository;

        public ChapterService()
        {
            _chapterRepository = new ChapterRepository();
            _projectRepository = new ProjectRepository();
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> CreateChapterAsync(
            int userId,
            int projectId,
            int chapterNo,
            string title,
            string? summary)
        {
            // Kiểm tra project có tồn tại và user có quyền không
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return (false, "Không tìm thấy dự án truyện", null);
            }

            if (project.AuthorId != userId)
            {
                return (false, "Bạn không có quyền thêm chương cho truyện này", null);
            }

            // Kiểm tra chapter number đã tồn tại chưa
            if (await _chapterRepository.ExistsAsync(projectId, chapterNo))
            {
                return (false, $"Chương số {chapterNo} đã tồn tại", null);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Tiêu đề chương là bắt buộc", null);
            }

            var newChapter = new Chapter
            {
                ProjectId = projectId,
                ChapterNo = chapterNo,
                Title = title.Trim(),
                Summary = summary?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var createdChapter = await _chapterRepository.CreateAsync(newChapter);
            return (true, "Tạo chương thành công", createdChapter);
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> GetChapterByIdAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương", null);
            }

            return (true, "Lấy thông tin chương thành công", chapter);
        }

        public async Task<(bool Success, string Message, List<Chapter>? Chapters)> GetChaptersByProjectIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return (false, "Không tìm thấy dự án truyện", null);
            }

            var chapters = await _chapterRepository.GetByProjectIdAsync(projectId);
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
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương", null);
            }

            // Kiểm tra quyền sở hữu
            if (!await _chapterRepository.IsOwnerAsync(chapterId, userId))
            {
                return (false, "Bạn không có quyền chỉnh sửa chương này", null);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Tiêu đề chương là bắt buộc", null);
            }

            // Nếu đổi số chương, kiểm tra trùng
            if (chapterNo.HasValue && chapterNo.Value != chapter.ChapterNo)
            {
                if (await _chapterRepository.ExistsAsync(chapter.ProjectId, chapterNo.Value))
                {
                    return (false, $"Chương số {chapterNo.Value} đã tồn tại", null);
                }
                chapter.ChapterNo = chapterNo.Value;
            }

            chapter.Title = title.Trim();
            chapter.Summary = summary?.Trim();

            var updatedChapter = await _chapterRepository.UpdateAsync(chapter);
            return (true, "Cập nhật chương thành công", updatedChapter);
        }

        public async Task<(bool Success, string Message)> DeleteChapterAsync(int userId, int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Không tìm thấy chương");
            }

            // Kiểm tra quyền sở hữu
            if (!await _chapterRepository.IsOwnerAsync(chapterId, userId))
            {
                return (false, "Bạn không có quyền xóa chương này");
            }

            var success = await _chapterRepository.DeleteAsync(chapterId);
            if (!success)
            {
                return (false, "Xóa chương thất bại");
            }

            return (true, "Xóa chương thành công");
        }

        public async Task<(bool Success, string Message, int NextChapterNo)> GetNextChapterNoAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return (false, "Không tìm thấy dự án truyện", 0);
            }

            var maxChapterNo = await _chapterRepository.GetMaxChapterNoAsync(projectId);
            return (true, "Lấy số chương tiếp theo thành công", maxChapterNo + 1);
        }
    }
}