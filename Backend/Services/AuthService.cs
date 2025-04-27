using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public AuthService(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return new AuthResult { Success = false, Error = "User not found" };

            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
                return new AuthResult { Success = false, Error = "Invalid password" };

            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResult
            {
                Success = true,
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return new AuthResult { Success = false, Error = "Username already exists" };

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return new AuthResult { Success = false, Error = "Email already exists" };

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResult
            {
                Success = true,
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            // In a real application, you might want to invalidate the token
            // This could involve adding it to a blacklist or updating the user's token version
            return true;
        }
    }
}
