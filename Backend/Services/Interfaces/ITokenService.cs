using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user, List<string> roles);
    }
} 