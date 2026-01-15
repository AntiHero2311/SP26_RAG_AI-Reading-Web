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
    public class ChapterVersionController : ControllerBase
    {
        private readonly ChapterVersionService _versionService;

        public ChapterVersionController()
        {
            _versionService = new ChapterVersionService();
        }

        /// <summary>
        /// Tạo phiên bản mới cho chương
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVersion([FromBody] CreateChapterVersionRequestDto request)
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

            var (success, message, version) = await _versionService.CreateVersionAsync(
                userId,
                request.ChapterId,
                request.RawContent
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new ChapterVersionResponseDto
            {
                VersionId = version!.VersionId,
                ChapterId = version.ChapterId,
                ChapterTitle = version.Chapter?.Title ?? "",
                VersionNumber = version.VersionNumber,
                RawContent = version.RawContent,
                WordCount = version.WordCount ?? 0,
                UploadDate = version.UploadDate ?? DateTime.MinValue,
                IsActive = version.IsActive ?? false,
                TotalAIJobs = version.Aijobs?.Count ?? 0,
                TotalAnalysisReports = version.AnalysisReports?.Count ?? 0,
                TotalChunks = version.ManuscriptChunks?.Count ?? 0
            };

            return CreatedAtAction(nameof(GetVersionById), new { id = version.VersionId }, new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Lấy thông tin chi tiết một phiên bản
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVersionById(int id)
        {
            var (success, message, version) = await _versionService.GetVersionByIdAsync(id);

            if (!success || version == null)
            {
                return NotFound(new { message });
            }

            var response = new ChapterVersionResponseDto
            {
                VersionId = version.VersionId,
                ChapterId = version.ChapterId,
                ChapterTitle = version.Chapter?.Title ?? "",
                VersionNumber = version.VersionNumber,
                RawContent = version.RawContent,
                WordCount = version.WordCount ?? 0,
                UploadDate = version.UploadDate ?? DateTime.MinValue,
                IsActive = version.IsActive ?? false,
                TotalAIJobs = version.Aijobs?.Count ?? 0,
                TotalAnalysisReports = version.AnalysisReports?.Count ?? 0,
                TotalChunks = version.ManuscriptChunks?.Count ?? 0
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Lấy danh sách tất cả phiên bản của một chương
        /// </summary>
        [HttpGet("chapter/{chapterId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVersionsByChapterId(int chapterId)
        {
            var (success, message, versions) = await _versionService.GetVersionsByChapterIdAsync(chapterId);

            if (!success || versions == null)
            {
                return BadRequest(new { message });
            }

            var versionList = versions.Select(v => new ChapterVersionResponseDto
            {
                VersionId = v.VersionId,
                ChapterId = v.ChapterId,
                ChapterTitle = v.Chapter?.Title ?? "",
                VersionNumber = v.VersionNumber,
                RawContent = v.RawContent,
                WordCount = v.WordCount ?? 0,
                UploadDate = v.UploadDate ?? DateTime.MinValue,
                IsActive = v.IsActive ?? false,
                TotalAIJobs = v.Aijobs?.Count ?? 0,
                TotalAnalysisReports = v.AnalysisReports?.Count ?? 0,
                TotalChunks = v.ManuscriptChunks?.Count ?? 0
            }).ToList();

            return Ok(new
            {
                message,
                data = versionList,
                count = versionList.Count
            });
        }

        /// <summary>
        /// Lấy phiên bản đang active của chương
        /// </summary>
        [HttpGet("chapter/{chapterId}/active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveVersion(int chapterId)
        {
            var (success, message, version) = await _versionService.GetActiveVersionAsync(chapterId);

            if (!success || version == null)
            {
                return NotFound(new { message });
            }

            var response = new ChapterVersionResponseDto
            {
                VersionId = version.VersionId,
                ChapterId = version.ChapterId,
                ChapterTitle = version.Chapter?.Title ?? "",
                VersionNumber = version.VersionNumber,
                RawContent = version.RawContent,
                WordCount = version.WordCount ?? 0,
                UploadDate = version.UploadDate ?? DateTime.MinValue,
                IsActive = version.IsActive ?? false,
                TotalAIJobs = version.Aijobs?.Count ?? 0,
                TotalAnalysisReports = version.AnalysisReports?.Count ?? 0,
                TotalChunks = version.ManuscriptChunks?.Count ?? 0
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Cập nhật nội dung phiên bản
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVersion(int id, [FromBody] UpdateChapterVersionRequestDto request)
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

            var (success, message, version) = await _versionService.UpdateVersionAsync(
                userId,
                id,
                request.RawContent
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new ChapterVersionResponseDto
            {
                VersionId = version!.VersionId,
                ChapterId = version.ChapterId,
                ChapterTitle = version.Chapter?.Title ?? "",
                VersionNumber = version.VersionNumber,
                RawContent = version.RawContent,
                WordCount = version.WordCount ?? 0,
                UploadDate = version.UploadDate ?? DateTime.MinValue,
                IsActive = version.IsActive ?? false,
                TotalAIJobs = version.Aijobs?.Count ?? 0,
                TotalAnalysisReports = version.AnalysisReports?.Count ?? 0,
                TotalChunks = version.ManuscriptChunks?.Count ?? 0
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Đặt phiên bản làm active (phiên bản hiện tại được hiển thị)
        /// </summary>
        [HttpPatch("{id}/set-active")]
        public async Task<IActionResult> SetActiveVersion(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message) = await _versionService.SetActiveVersionAsync(userId, id);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Xóa phiên bản (không thể xóa version đang active)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVersion(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message) = await _versionService.DeleteVersionAsync(userId, id);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}