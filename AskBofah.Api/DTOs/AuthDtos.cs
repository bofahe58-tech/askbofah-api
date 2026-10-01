using System.ComponentModel.DataAnnotations;

namespace AskBofah.Api.DTOs
{
    // ============================================================
    // REQUESTS
    // ============================================================
    public class RegisterRequest
    {
        [Required, MinLength(2), MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateRoleRequest
    {
        public bool IsAdmin { get; set; }
    }

    // ============================================================
    // RESPONSES
    // ============================================================
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PlanTier { get; set; } = "Free";
        public bool IsAdmin { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
        public int MessagesSentToday { get; set; }
        public int DailyMessageLimit { get; set; } = 50;
    }

    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PlanTier { get; set; } = "Free";
        public bool IsAdmin { get; set; }
        public int MessagesSentToday { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }
    }
}