using Repository;
using Repository.Models;
using Service.Helpers; // Import Helper mã hóa

namespace Service
{
    public class ProjectService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly UserRepository _userRepository;

        // 1. SỬA: Tiêm Repository qua Constructor (DI)
        public ProjectService(ProjectRepository projectRepo, UserRepository userRepo)
        {
            _projectRepository = projectRepo;
            _userRepository = userRepo;
        }

        public async Task<(bool Success, string Message, Project? Project)> CreateProjectAsync(
            int authorId,
            string title,
            string? summary,
            string? coverImageUrl)
        {
            // 2. Lấy thông tin Author để lấy Key mã hóa
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null) return (false, "Không tìm thấy tác giả", null);

            // Kiểm tra xem User đã có Key chưa, nếu chưa (user cũ) thì báo lỗi hoặc tạo mới
            if (string.IsNullOrEmpty(author.DataEncryptionKey))
                return (false, "Lỗi bảo mật: Tài khoản chưa có khóa mã hóa", null);

            string encryptionKey = author.DataEncryptionKey;

            var newProject = new Project
            {
                AuthorId = authorId,
                // 3. MÃ HÓA CÁC TRƯỜNG CẦN THIẾT
                Title = SecurityHelper.Encrypt(title.Trim(), encryptionKey),
                Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), encryptionKey) : null,
                CoverImageUrl = coverImageUrl != null ? SecurityHelper.Encrypt(coverImageUrl.Trim(), encryptionKey) : null,

                Status = "Draft",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.CreateAsync(newProject);

            // Trả về object đã giải mã (để Controller hiển thị ngay)
            createdProject.Title = title;
            createdProject.Summary = summary;
            createdProject.CoverImageUrl = coverImageUrl;
            createdProject.Author = author; // Gán lại để mapping

            return (true, "Tạo dự án truyện thành công", createdProject);
        }

        public async Task<(bool Success, string Message, List<Project>? Projects)> GetMyProjectsAsync(
            int authorId,
            bool includeDraft = true)
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null) return (false, "User không tồn tại", null);

            var projects = await _projectRepository.GetByAuthorIdAsync(authorId, includeDraft);

            // 4. GIẢI MÃ DANH SÁCH
            foreach (var p in projects)
            {
                p.Title = SecurityHelper.Decrypt(p.Title, author.DataEncryptionKey);
                p.Summary = SecurityHelper.Decrypt(p.Summary, author.DataEncryptionKey);
                p.CoverImageUrl = SecurityHelper.Decrypt(p.CoverImageUrl, author.DataEncryptionKey);
            }

            return (true, "Lấy danh sách truyện thành công", projects);
        }

        // Hàm lấy chi tiết (Dùng cho cả Public xem hoặc Author xem)
        public async Task<(bool Success, string Message, Project? Project)> GetProjectByIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return (false, "Không tìm thấy truyện", null);

            // Cần lấy Author của truyện này để lấy Key giải mã
            var author = await _userRepository.GetByIdAsync(project.AuthorId);

            // 5. GIẢI MÃ
            project.Title = SecurityHelper.Decrypt(project.Title, author.DataEncryptionKey);
            project.Summary = SecurityHelper.Decrypt(project.Summary, author.DataEncryptionKey);
            project.CoverImageUrl = SecurityHelper.Decrypt(project.CoverImageUrl, author.DataEncryptionKey);

            return (true, "Thành công", project);
        }

        public async Task<(bool Success, string Message, Project? Project)> UpdateProjectAsync(
            int projectId,
            int userId, // Người đang thực hiện sửa (phải là Author)
            string title,
            string? summary,
            string? coverImageUrl,
            string? status)
        {
            var isOwner = await _projectRepository.IsOwnerAsync(projectId, userId);
            if (!isOwner) return (false, "Bạn không có quyền chỉnh sửa truyện này", null);

            var project = await _projectRepository.GetByIdAsync(projectId);
            var author = await _userRepository.GetByIdAsync(userId); // Lấy Key

            project.Title = SecurityHelper.Encrypt(title.Trim(), author.DataEncryptionKey);
            project.Summary = summary != null ? SecurityHelper.Encrypt(summary.Trim(), author.DataEncryptionKey) : null;
            project.CoverImageUrl = coverImageUrl != null ? SecurityHelper.Encrypt(coverImageUrl.Trim(), author.DataEncryptionKey) : null;

            if (!string.IsNullOrWhiteSpace(status)) project.Status = status;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);

            project.Title = title;
            project.Summary = summary;
            project.CoverImageUrl = coverImageUrl;

            return (true, "Cập nhật truyện thành công", project);
        }

        public async Task<(bool Success, string Message)> DeleteProjectAsync(int projectId, int userId)
        {
            var isOwner = await _projectRepository.IsOwnerAsync(projectId, userId);
            if (!isOwner) return (false, "Bạn không có quyền xóa truyện này");

            var success = await _projectRepository.SoftDeleteAsync(projectId);
            return success ? (true, "Xóa truyện thành công") : (false, "Lỗi khi xóa");
        }
    }
}