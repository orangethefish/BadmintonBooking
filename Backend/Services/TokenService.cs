using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BadmintonBooking.API.Models;
using Microsoft.IdentityModel.Tokens;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _logger;

        public TokenService(IConfiguration configuration, ILoggerService logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateJwtToken(User user, List<string> roles)
        {
            try
            {
                _logger.Info($"Generating JWT token for user: {user.Username}");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                };

                // Add each role as a separate claim
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                _logger.Info($"JWT token generated successfully for user: {user.Username}");
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to generate JWT token for user: {user.Username}", ex);
                throw new Exception($"Failed to generate JWT token: {ex.Message}", ex);
            }
        }
    }
}
