using Repository;
using Repository.Models;
using Service.Helpers; // Import SecurityHelper

namespace Service
{
    public class ChapterService
    {
        private readonly ChapterRepository _chapterRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly UserRepository _userRepository; // Cần thêm cái này để lấy Key

        // 1. SỬA: Constructor dùng DI
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
            int userId, // ID của người đang thao tác (Author)
            int projectId,
            int chapterNo,
            string title,
            string? summary)
        {
            // Kiểm tra project
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy dự án truyện", null);

            // Kiểm tra quyền (chỉ Author mới được tạo chapter)
            if (project.AuthorId != userId) return (false, "Bạn không có quyền thêm chương", null);

            // Lấy Key của Author
            var author = await _userRepository.GetByIdAsync(userId);
            if (string.IsNullOrEmpty(author?.DataEncryptionKey))
                return (false, "Lỗi: Tài khoản chưa có khóa mã hóa", null);

            string encryptionKey = author.DataEncryptionKey;

            // Kiểm tra trùng chapter no
            if (await _chapterRepository.ExistsAsync(projectId, chapterNo))
                return (false, $"Chương số {chapterNo} đã tồn tại", null);

            if (string.IsNullOrWhiteSpace(title)) return (false, "Tiêu đề chương là bắt buộc", null);

            var newChapter = new Chapter
            {
                ProjectId = projectId,
                ChapterNo = chapterNo,
                // 2. MÃ HÓA TRƯỚC KHI LƯU
                Title = SecurityHelper.Encrypt(title.Trim(), encryptionKey),
                Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), encryptionKey) : null,
                CreatedAt = DateTime.UtcNow
            };

            var createdChapter = await _chapterRepository.CreateAsync(newChapter);

            // Gán lại bản rõ để Controller trả về cho User xem ngay
            createdChapter.Title = title;
            createdChapter.Summary = summary;
            createdChapter.Project = project; // Map ngược lại để hiển thị ProjectTitle

            return (true, "Tạo chương thành công", createdChapter);
        }

        public async Task<(bool Success, string Message, Chapter? Chapter)> GetChapterByIdAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return (false, "Không tìm thấy chương", null);

            // Phải lấy Project -> Author -> Key
            var project = await _projectRepository.GetByIdAsync(chapter.ProjectId);
            var author = await _userRepository.GetByIdAsync(project.AuthorId);

            // 3. GIẢI MÃ ĐỂ XEM CHI TIẾT
            chapter.Title = SecurityHelper.Decrypt(chapter.Title, author.DataEncryptionKey);
            chapter.Summary = SecurityHelper.Decrypt(chapter.Summary, author.DataEncryptionKey);

            return (true, "Thành công", chapter);
        }

        public async Task<(bool Success, string Message, List<Chapter>? Chapters)> GetChaptersByProjectIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy dự án truyện", null);

            // Lấy Key của tác giả cuốn truyện này
            var author = await _userRepository.GetByIdAsync(project.AuthorId);

            var chapters = await _chapterRepository.GetByProjectIdAsync(projectId);

            // 4. GIẢI MÃ DANH SÁCH
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

            // Check quyền: Chapter -> Project -> Check AuthorId
            // (Hoặc dùng hàm IsOwnerAsync có sẵn trong Repo nếu Repo check join bảng Project)
            var project = await _projectRepository.GetByIdAsync(chapter.ProjectId);
            if (project.AuthorId != userId) return (false, "Không có quyền chỉnh sửa", null);

            var author = await _userRepository.GetByIdAsync(userId); // Lấy Key

            if (string.IsNullOrWhiteSpace(title)) return (false, "Tiêu đề bắt buộc", null);

            // Check trùng số chương
            if (chapterNo.HasValue && chapterNo.Value != chapter.ChapterNo)
            {
                if (await _chapterRepository.ExistsAsync(chapter.ProjectId, chapterNo.Value))
                    return (false, $"Chương số {chapterNo.Value} đã tồn tại", null);
                chapter.ChapterNo = chapterNo.Value;
            }

            // 5. MÃ HÓA KHI UPDATE
            chapter.Title = SecurityHelper.Encrypt(title.Trim(), author.DataEncryptionKey);
            chapter.Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), author.DataEncryptionKey) : null;
            chapter.UpdatedAt = DateTime.UtcNow;

            var updatedChapter = await _chapterRepository.UpdateAsync(chapter);

            // Trả về bản rõ
            updatedChapter.Title = title;
            updatedChapter.Summary = summary;

            return (true, "Cập nhật chương thành công", updatedChapter);
        }

        public async Task<(bool Success, string Message)> DeleteChapterAsync(int userId, int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId);
            if (chapter == null) return (false, "Không tìm thấy chương");

            // Check quyền thủ công hoặc qua Repo
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