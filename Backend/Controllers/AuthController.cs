using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Controllers
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
        public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
        {
            var model = new RegisterModel
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            };

            var result = await _authService.RegisterAsync(model);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User not found" });

            var result = await _authService.LogoutAsync(userId);
            if (!result)
                return BadRequest(new { message = "Logout failed" });

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
