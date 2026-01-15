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
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController()
        {
            _userService = new UserService();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, user) = await _userService.GetUserProfileAsync(userId);

            if (!success)
            {
                return NotFound(new { message });
            }

            var response = new UserProfileResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt ?? DateTime.MinValue,
                IsActive = user.IsActive ?? true
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
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

            var (success, message, user) = await _userService.UpdateUserProfileAsync(
                userId,
                request.FullName,
                request.AvatarUrl
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new UserProfileResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt ?? DateTime.MinValue,
                IsActive = user.IsActive ?? true
            };

            return Ok(new
            {
                message,
                data = response
            });
        }
    }
}