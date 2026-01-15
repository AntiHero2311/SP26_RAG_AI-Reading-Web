using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;
using System.Security.Claims;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController()
        {
            _projectService = new ProjectService();
        }

        /// <summary>
        /// Tạo dự án truyện mới (Title, Summary)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, project) = await _projectService.CreateProjectAsync(
                authorId,
                request.Title,
                request.Summary,
                request.CoverImageUrl
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new ProjectResponseDto
            {
                ProjectId = project!.ProjectId,
                AuthorId = project.AuthorId,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = project.UpdatedAt,
                TotalChapters = project.Chapters?.Count ?? 0,
                TotalChatSessions = project.ChatSessions?.Count ?? 0,
                Genres = project.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            };

            return CreatedAtAction(nameof(GetProjectById), new { id = project.ProjectId }, new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Lấy danh sách truyện của tôi (bao gồm Draft)
        /// </summary>
        [HttpGet("my-projects")]
        public async Task<IActionResult> GetMyProjects([FromQuery] bool includeDraft = true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, projects) = await _projectService.GetMyProjectsAsync(authorId, includeDraft);

            if (!success || projects == null)
            {
                return BadRequest(new { message });
            }

            var projectList = projects.Select(p => new ProjectResponseDto
            {
                ProjectId = p.ProjectId,
                AuthorId = p.AuthorId,
                AuthorName = p.Author?.FullName ?? "",
                Title = p.Title,
                Summary = p.Summary,
                CoverImageUrl = p.CoverImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = p.UpdatedAt,
                TotalChapters = p.Chapters?.Count ?? 0,
                TotalChatSessions = p.ChatSessions?.Count ?? 0,
                Genres = p.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            }).ToList();

            return Ok(new
            {
                message,
                data = projectList,
                count = projectList.Count
            });
        }

        /// <summary>
        /// Lấy thông tin chi tiết một truyện
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var projectRepo = new Repository.ProjectRepository();
            var project = await projectRepo.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound(new { message = "Không tìm thấy truyện" });
            }

            var response = new ProjectResponseDto
            {
                ProjectId = project.ProjectId,
                AuthorId = project.AuthorId,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = project.UpdatedAt,
                TotalChapters = project.Chapters?.Count ?? 0,
                TotalChatSessions = project.ChatSessions?.Count ?? 0,
                Genres = project.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            };

            return Ok(new
            {
                message = "Lấy thông tin truyện thành công",
                data = response
            });
        }

        /// <summary>
        /// Cập nhật thông tin truyện (Bìa, Tóm tắt)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, project) = await _projectService.UpdateProjectAsync(
                id,
                userId,
                request.Title,
                request.Summary,
                request.CoverImageUrl,
                request.Status
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new ProjectResponseDto
            {
                ProjectId = project!.ProjectId,
                AuthorId = project.AuthorId,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = project.UpdatedAt,
                TotalChapters = project.Chapters?.Count ?? 0,
                TotalChatSessions = project.ChatSessions?.Count ?? 0,
                Genres = project.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Xóa truyện (Soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message) = await _projectService.DeleteProjectAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}