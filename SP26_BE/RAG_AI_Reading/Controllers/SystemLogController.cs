using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;
using System.Security.Claims;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SystemLogController : ControllerBase
    {
        private readonly SystemLogService _logService;

        public SystemLogController(SystemLogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Ghi nhật ký sự kiện hệ thống
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] CreateSystemLogRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? actorId = null;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                actorId = userId;
            }

            var (success, message, log) = await _logService.CreateLogAsync(
                actorId,
                request.ActionType,
                request.Description
            );

            if (!success)
                return BadRequest(new { message });

            var response = MapToDto(log!);
            return CreatedAtAction(nameof(GetLogById), new { id = log.LogId }, new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Lấy chi tiết một nhật ký
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogById(int id)
        {
            var (success, message, log) = await _logService.GetLogByIdAsync(id);

            if (!success || log == null)
                return NotFound(new { message });

            return Ok(new
            {
                message,
                data = MapToDto(log)
            });
        }

        /// <summary>
        /// Lấy danh sách nhật ký với bộ lọc (Action Type, Actor, Date Range)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? actionType = null,
            [FromQuery] int? actorId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var (success, message, logs, totalCount, totalPages) = await _logService.GetLogsAsync(
                pageNumber,
                pageSize,
                actionType,
                actorId,
                startDate,
                endDate
            );

            if (!success || logs == null)
                return BadRequest(new { message });

            var logList = logs.Select(MapToDto).ToList();

            return Ok(new
            {
                message,
                data = logList,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }

        /// <summary>
        /// Lấy danh sách nhật ký theo loại hành động
        /// </summary>
        [HttpGet("by-action-type/{actionType}")]
        public async Task<IActionResult> GetLogsByActionType(
            string actionType,
            [FromQuery] int limit = 100)
        {
            var (success, message, logs) = await _logService.GetLogsByActionTypeAsync(actionType, limit);

            if (!success || logs == null)
                return BadRequest(new { message });

            var logList = logs.Select(MapToDto).ToList();

            return Ok(new
            {
                message,
                data = logList,
                count = logList.Count
            });
        }

        /// <summary>
        /// Lấy danh sách nhật ký theo người dùng
        /// </summary>
        [HttpGet("by-actor/{actorId}")]
        public async Task<IActionResult> GetLogsByActor(
            int actorId,
            [FromQuery] int limit = 100)
        {
            var (success, message, logs) = await _logService.GetLogsByActorAsync(actorId, limit);

            if (!success || logs == null)
                return BadRequest(new { message });

            var logList = logs.Select(MapToDto).ToList();

            return Ok(new
            {
                message,
                data = logList,
                count = logList.Count
            });
        }

        /// <summary>
        /// Lấy tổng số nhật ký trong hệ thống
        /// </summary>
        [HttpGet("stats/total")]
        public async Task<IActionResult> GetTotalLogs()
        {
            var (success, message, totalLogs) = await _logService.GetTotalCountAsync();

            if (!success)
                return BadRequest(new { message });

            return Ok(new
            {
                message,
                totalLogs
            });
        }

        /// <summary>
        /// Xóa một nhật ký
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var (success, message) = await _logService.DeleteLogAsync(id);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        /// <summary>
        /// Xóa nhật ký cũ hơn N ngày (Dùng cho dọn dẹp)
        /// </summary>
        [HttpDelete("cleanup/older-than/{daysOld}")]
        public async Task<IActionResult> DeleteOldLogs(int daysOld)
        {
            var (success, message) = await _logService.DeleteOldLogsAsync(daysOld);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        private SystemLogResponseDto MapToDto(Repository.Models.SystemLog log)
        {
            return new SystemLogResponseDto
            {
                LogId = log.LogId,
                ActorId = log.ActorId,
                ActorName = log.Actor?.FullName ?? "System",
                ActionType = log.ActionType,
                Description = log.Description,
                Timestamp = log.Timestamp
            };
        }
    }
}
