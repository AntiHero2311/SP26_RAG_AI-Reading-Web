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

        // 1. SỬA: Dùng DI Constructor
        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int authorId)) return Unauthorized();

            var (success, message, project) = await _projectService.CreateProjectAsync(
                authorId, request.Title, request.Summary, request.CoverImageUrl
            );

            if (!success) return BadRequest(new { message });

            return Ok(new { message, data = MapToDto(project!) });
        }

        [HttpGet("my-projects")]
        public async Task<IActionResult> GetMyProjects([FromQuery] bool includeDraft = true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int authorId)) return Unauthorized();

            var (success, message, projects) = await _projectService.GetMyProjectsAsync(authorId, includeDraft);
            if (!success) return BadRequest(new { message });

            var listDto = projects!.Select(MapToDto).ToList();
            return Ok(new { message, data = listDto, count = listDto.Count });
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép khách xem
        public async Task<IActionResult> GetProjectById(int id)
        {
            // Gọi qua Service để được xử lý Giải mã
            var (success, message, project) = await _projectService.GetProjectByIdAsync(id);

            if (!success) return NotFound(new { message });

            return Ok(new { message, data = MapToDto(project!) });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, message, project) = await _projectService.UpdateProjectAsync(
                id, userId, request.Title, request.Summary, request.CoverImageUrl, request.Status
            );

            if (!success) return BadRequest(new { message });
            return Ok(new { message, data = MapToDto(project!) });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, message) = await _projectService.DeleteProjectAsync(id, userId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // Helper Map DTO cho gọn code
        private ProjectResponseDto MapToDto(Repository.Models.Project p)
        {
            return new ProjectResponseDto
            {
                ProjectId = p.ProjectId,
                AuthorId = p.AuthorId,
                AuthorName = p.Author?.FullName ?? "",
                Title = p.Title, // Đã được giải mã trong Service
                Summary = p.Summary, // Đã được giải mã
                CoverImageUrl = p.CoverImageUrl, // Đã được giải mã
                Status = p.Status,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = p.UpdatedAt,
                Genres = p.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            };
        }
    }
}