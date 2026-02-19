using System;

namespace ExpenseTracker.Models.Dto
{
    public class AuthResponse
    {
        public string Token { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string Email { get; set; } = "";
    }
}
