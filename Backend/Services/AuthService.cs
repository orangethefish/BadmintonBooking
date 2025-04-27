using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public AuthService(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                throw new Exception("User not found");

            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
                throw new Exception("Invalid password");

            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new Exception("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponse
            {
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
    }
}
