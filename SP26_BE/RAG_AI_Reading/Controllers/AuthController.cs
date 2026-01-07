using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(IConfiguration configuration)
        {
            _authService = new AuthService(configuration);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _authService.RegisterAsync(
                request.FullName,
                request.Email,
                request.Password,
                request.AvatarUrl
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var token = _authService.GenerateJwtToken(user!);

            var response = new AuthResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Token = token
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _authService.LoginAsync(
                request.Email,
                request.Password
            );

            if (!success)
            {
                return Unauthorized(new { message });
            }

            var token = _authService.GenerateJwtToken(user!);

            var response = new AuthResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Token = token
            };

            return Ok(new
            {
                message,
                data = response
            });
        }
    }
}
