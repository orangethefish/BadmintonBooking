using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILoggerService _logger;
        private readonly IRoleService _roleService;

        public AuthService(
            ApplicationDbContext context, 
            ITokenService tokenService, 
            ILoggerService logger,
            IRoleService roleService)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
            _roleService = roleService;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                _logger.Info($"Attempting login for user with email: {email}");
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.Warning($"Login failed: User not found for email: {email}");
                    return new AuthResult { Success = false, Error = "User not found" };
                }

                var passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    _logger.Warning($"Login failed: Invalid password for user: {user.Username}");
                    return new AuthResult { Success = false, Error = "Invalid password" };
                }

                // Get user roles
                var roles = await _roleService.GetUserRolesAsync(user.Id);
                
                var token = _tokenService.GenerateJwtToken(user, roles);
                _logger.Info($"Login successful for user: {user.Username}");

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    Username = user.Username,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error for email: {email}", ex);
                return new AuthResult { Success = false, Error = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            try
            {
                _logger.Info($"Attempting registration for user: {model.Username}");
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    _logger.Warning($"Registration failed: Username already exists: {model.Username}");
                    return new AuthResult { Success = false, Error = "Username already exists" };
                }

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.Warning($"Registration failed: Email already exists: {model.Email}");
                    return new AuthResult { Success = false, Error = "Email already exists" };
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign roles based on account type
                string roleName = "User"; // Default role
                
                if (model.AccountType.Equals("Owner", StringComparison.OrdinalIgnoreCase))
                {
                    roleName = "Owner";
                }
                
                await _roleService.AddUserToRoleAsync(user.Id, roleName);
                
                // Get user roles
                var roles = await _roleService.GetUserRolesAsync(user.Id);
                
                var token = _tokenService.GenerateJwtToken(user, roles);
                _logger.Info($"Registration successful for user: {user.Username} with role: {roleName}");

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    Username = user.Username,
                    Roles = roles
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Registration error for user: {model.Username}", ex);
                return new AuthResult { Success = false, Error = $"An error occurred: {ex.Message}" };
            }
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
            try
            {
                _logger.Info($"User logout: {userId}");
                // In a real application, you might want to invalidate the token
                // This could involve adding it to a blacklist or updating the user's token version
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Logout error for user: {userId}", ex);
                return false;
            }
        }
    }
}
