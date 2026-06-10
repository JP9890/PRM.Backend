using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authResponse = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (authResponse == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(authResponse);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Stateless JWT logout is handled on the client side by removing the token.
            // We just return a success message here.
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PRMTool.Application.DTOs.ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = ResolveUsername();
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var success = await _authService.ChangePasswordAsync(username, request.OldPassword ?? string.Empty, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Check your current password and ensure the new password meets requirements (8+ chars, uppercase, number)." });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        private string? ResolveUsername()
        {
            return User.Identity?.Name
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("unique_name")
                ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/name", System.StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
