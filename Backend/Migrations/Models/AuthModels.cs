using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        // Add AccountType to determine if User or Owner account
        [Required]
        public string AccountType { get; set; } = "User"; // Default to User if not specified
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccountType { get; set; } = "User"; // Default to User if not specified
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
