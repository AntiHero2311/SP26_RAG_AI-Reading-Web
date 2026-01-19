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
    public class ChapterController : ControllerBase
    {
        private readonly ChapterService _chapterService;

        public ChapterController(ChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, message, chapter) = await _chapterService.CreateChapterAsync(
                userId, request.ProjectId, request.ChapterNo, request.Title, request.Summary
            );

            if (!success) return BadRequest(new { message });

            var response = MapToDto(chapter!);

            return CreatedAtAction(nameof(GetChapterById), new { id = chapter!.ChapterId }, new
            {
                message,
                data = response
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChapterById(int id)
        {
            var (success, message, chapter) = await _chapterService.GetChapterByIdAsync(id);

            if (!success || chapter == null) return NotFound(new { message });

            return Ok(new { message, data = MapToDto(chapter) });
        }

        [HttpGet("project/{projectId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChaptersByProjectId(int projectId)
        {
            var (success, message, chapters) = await _chapterService.GetChaptersByProjectIdAsync(projectId);

            if (!success) return BadRequest(new { message });

            var chapterList = chapters!.Select(MapToDto).ToList();

            return Ok(new { message, data = chapterList, count = chapterList.Count });
        }

        [HttpGet("project/{projectId}/next-chapter-no")]
        public async Task<IActionResult> GetNextChapterNo(int projectId)
        {
            var (success, message, nextChapterNo) = await _chapterService.GetNextChapterNoAsync(projectId);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, nextChapterNo });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChapter(int id, [FromBody] UpdateChapterRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, message, chapter) = await _chapterService.UpdateChapterAsync(
                userId, id, request.Title, request.Summary, request.ChapterNo
            );

            if (!success) return BadRequest(new { message });

            return Ok(new { message, data = MapToDto(chapter!) });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, message) = await _chapterService.DeleteChapterAsync(userId, id);

            if (!success) return BadRequest(new { message });

            return Ok(new { message });
        }

        // Helper Map DTO
        private ChapterResponseDto MapToDto(Repository.Models.Chapter c)
        {
            return new ChapterResponseDto
            {
                ChapterId = c.ChapterId,
                ProjectId = c.ProjectId,
                ProjectTitle = c.Project?.Title ?? "",
                ChapterNo = c.ChapterNo,
                Title = c.Title,
                Summary = c.Summary,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = c.UpdatedAt,
                TotalVersions = c.ChapterVersions?.Count ?? 0
            };
        }
    }
}