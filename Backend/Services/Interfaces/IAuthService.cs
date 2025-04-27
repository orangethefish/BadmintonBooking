using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RegisterAsync(RegisterModel model);
        Task<bool> LogoutAsync(string userId);
    }
}
