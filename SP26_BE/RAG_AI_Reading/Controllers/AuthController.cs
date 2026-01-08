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

            var accessToken = _authService.GenerateJwtToken(user!);
            var refreshToken = _authService.GenerateRefreshToken();

            // Lưu refresh token vào database
            await _authService.SaveRefreshTokenAsync(user!, refreshToken);

            var response = new AuthResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Token = accessToken
            };

            return Ok(new
            {
                message,
                data = response,
                refreshToken
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

            var accessToken = _authService.GenerateJwtToken(user!);
            var refreshToken = _authService.GenerateRefreshToken();

            // Lưu refresh token vào database
            await _authService.SaveRefreshTokenAsync(user!, refreshToken);

            var response = new AuthResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Token = accessToken
            };

            return Ok(new
            {
                message,
                data = response,
                refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, user) = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!success)
            {
                return Unauthorized(new { message });
            }

            // Generate new tokens
            var newAccessToken = _authService.GenerateJwtToken(user!);
            var newRefreshToken = _authService.GenerateRefreshToken();

            // Update refresh token in database
            await _authService.SaveRefreshTokenAsync(user!, newRefreshToken);

            var response = new AuthResponseDto
            {
                UserId = user!.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                Token = newAccessToken
            };

            return Ok(new
            {
                message = "Cập lại token mới thành công",
                data = response,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _authService.ForgotPasswordAsync(request.Email);

            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _authService.ResetPasswordAsync(
                request.Token,
                request.NewPassword
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }
}
