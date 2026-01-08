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

            // Get userId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, project) = await _projectService.CreateProjectAsync(
                authorId,
                request.Title,
                request.Genre,
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
                AuthorId = project.AuthorId ?? 0,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Genre = project.Genre,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                TotalVersions = project.ManuscriptVersions?.Count ?? 0,
                TotalComments = project.Comments?.Count ?? 0,
                TotalRatings = project.Ratings?.Count ?? 0,
                AverageRating = project.Ratings?.Any() == true 
                    ? project.Ratings.Average(r => r.Score) 
                    : 0
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
            // Get userId from JWT token
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
                AuthorId = p.AuthorId ?? 0,
                AuthorName = p.Author?.FullName ?? "",
                Title = p.Title,
                Genre = p.Genre,
                Summary = p.Summary,
                CoverImageUrl = p.CoverImageUrl,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                TotalVersions = p.ManuscriptVersions?.Count ?? 0,
                TotalComments = p.Comments?.Count ?? 0,
                TotalRatings = p.Ratings?.Count ?? 0,
                AverageRating = p.Ratings?.Any() == true 
                    ? p.Ratings.Average(r => r.Score) 
                    : 0
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
                AuthorId = project.AuthorId ?? 0,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Genre = project.Genre,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                TotalVersions = project.ManuscriptVersions?.Count ?? 0,
                TotalComments = project.Comments?.Count ?? 0,
                TotalRatings = project.Ratings?.Count ?? 0,
                AverageRating = project.Ratings?.Any() == true 
                    ? project.Ratings.Average(r => r.Score) 
                    : 0
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

            // Get userId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, project) = await _projectService.UpdateProjectAsync(
                id,
                userId,
                request.Title,
                request.Genre,
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
                AuthorId = project.AuthorId ?? 0,
                AuthorName = project.Author?.FullName ?? "",
                Title = project.Title,
                Genre = project.Genre,
                Summary = project.Summary,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                TotalVersions = project.ManuscriptVersions?.Count ?? 0,
                TotalComments = project.Comments?.Count ?? 0,
                TotalRatings = project.Ratings?.Count ?? 0,
                AverageRating = project.Ratings?.Any() == true 
                    ? project.Ratings.Average(r => r.Score) 
                    : 0
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
            // Get userId from JWT token
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