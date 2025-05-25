namespace BadmintonBooking.API.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
        public string Error { get; set; }
    }
}
