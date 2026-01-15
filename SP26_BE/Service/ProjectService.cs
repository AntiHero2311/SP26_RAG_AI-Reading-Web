using Repository;
using Repository.Models;

namespace Service
{
    public class ProjectService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly UserRepository _userRepository;

        public ProjectService()
        {
            _projectRepository = new ProjectRepository();
            _userRepository = new UserRepository();
        }

        public async Task<(bool Success, string Message, Project? Project)> CreateProjectAsync(
            int authorId, 
            string title, 
            string? summary, 
            string? coverImageUrl)
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
            {
                return (false, "Không tìm thấy tác giả", null);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Tiêu đề là bắt buộc", null);
            }

            var newProject = new Project
            {
                AuthorId = authorId,
                Title = title.Trim(),
                Summary = summary?.Trim(),
                CoverImageUrl = coverImageUrl?.Trim(),
                Status = "Draft",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.CreateAsync(newProject);
            return (true, "Tạo dự án truyện thành công", createdProject);
        }

        public async Task<(bool Success, string Message, List<Project>? Projects)> GetMyProjectsAsync(
            int authorId, 
            bool includeDraft = true)
        {
            var projects = await _projectRepository.GetByAuthorIdAsync(authorId, includeDraft);
            return (true, "Lấy danh sách truyện thành công", projects);
        }

        public async Task<(bool Success, string Message, Project? Project)> UpdateProjectAsync(
            int projectId,
            int userId,
            string title,
            string? summary,
            string? coverImageUrl,
            string? status)
        {
            var isOwner = await _projectRepository.IsOwnerAsync(projectId, userId);
            if (!isOwner)
            {
                return (false, "Bạn không có quyền chỉnh sửa truyện này", null);
            }

            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return (false, "Không tìm thấy truyện", null);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Tiêu đề là bắt buộc", null);
            }

            project.Title = title.Trim();
            project.Summary = summary?.Trim();
            project.CoverImageUrl = coverImageUrl?.Trim();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var validStatuses = new[] { "Draft", "Published", "Completed", "Paused" };
                if (validStatuses.Contains(status))
                {
                    project.Status = status;
                }
            }

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return (true, "Cập nhật truyện thành công", updatedProject);
        }

        public async Task<(bool Success, string Message)> DeleteProjectAsync(int projectId, int userId)
        {
            var isOwner = await _projectRepository.IsOwnerAsync(projectId, userId);
            if (!isOwner)
            {
                return (false, "Bạn không có quyền xóa truyện này");
            }

            var success = await _projectRepository.SoftDeleteAsync(projectId);
            if (!success)
            {
                return (false, "Không tìm thấy truyện");
            }

            return (true, "Xóa truyện thành công");
        }
    }
}