using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service;
using System.Security.Claims;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly SystemLogService _logService;

        public AuthController(AuthService authService, SystemLogService logService)
        {
            _authService = authService;
            _logService = logService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, user) = await _authService.RegisterAsync(
                request.FullName,
                request.Email,
                request.Password,
                request.AvatarUrl
            );

            if (!success)
                return BadRequest(new { message });

            var accessToken = _authService.GenerateJwtToken(user!);
            var refreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshTokenAsync(user!, refreshToken);

            await _logService.CreateLogAsync(
                user!.UserId,
                "REGISTER",
                $"User {user.Email} đã đăng ký tài khoản"
            );

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
                return BadRequest(ModelState);

            var (success, message, user) = await _authService.LoginAsync(
                request.Email,
                request.Password
            );

            if (!success)
                return Unauthorized(new { message });

            var accessToken = _authService.GenerateJwtToken(user!);
            var refreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshTokenAsync(user!, refreshToken);

            await _logService.CreateLogAsync(
                user!.UserId,
                "LOGIN",
                $"User {user.Email} đã đăng nhập"
            );

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
                return BadRequest(ModelState);

            var (success, message, user) = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!success)
                return Unauthorized(new { message });

            var newAccessToken = _authService.GenerateJwtToken(user!);
            var newRefreshToken = _authService.GenerateRefreshToken();

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
                message = "Cấp lại token mới thành công",
                data = response,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _authService.ForgotPasswordAsync(request.Email);

            // Luôn trả về OK để bảo mật (tránh check email tồn tại)
            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.GetUserByPasswordResetTokenAsync(request.Token);
            if (user == null)
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn" });

            var (success, message) = await _authService.ResetPasswordAsync(
                request.Token,
                request.NewPassword
            );

            if (!success)
                return BadRequest(new { message });

            await _logService.CreateLogAsync(
                user.UserId,
                "CHANGE_PASSWORD",
                $"User {user.Email} đã đổi mật khẩu (reset)"
            );

            return Ok(new { message });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var (success, message) = await _authService.ChangePasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!success)
                return BadRequest(new { message });

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            await _logService.CreateLogAsync(
                userId,
                "CHANGE_PASSWORD",
                $"User {userEmail} đã đổi mật khẩu"
            );

            return Ok(new { message });
        }
    }
}