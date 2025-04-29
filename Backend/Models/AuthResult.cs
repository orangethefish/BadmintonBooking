namespace BadmintonBooking.API.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Error { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
