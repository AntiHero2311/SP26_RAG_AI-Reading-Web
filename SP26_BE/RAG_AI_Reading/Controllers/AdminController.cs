using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController()
        {
            _adminService = new AdminService();
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null,
            [FromQuery] bool includeInactive = false)
        {
            var (success, message, users, totalCount, totalPages) = await _adminService.GetAllUsersAsync(
                pageNumber,
                pageSize,
                searchTerm,
                role,
                includeInactive
            );

            if (!success || users == null)
            {
                return BadRequest(new { message });
            }

            var userList = users.Select(u => new UserListItemDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                IsActive = u.IsActive ?? true,
                CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                TotalProjects = u.Projects?.Count ?? 0
            }).ToList();

            return Ok(new
            {
                message,
                data = userList,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }
    }
}