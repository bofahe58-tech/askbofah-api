using System.Text.Json.Serialization;

namespace AskBofah.Models
{
    public class AdminUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("planTier")]
        public string PlanTier { get; set; } = "Free";

        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonPropertyName("messagesSentToday")]
        public int MessagesSentToday { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [JsonPropertyName("subscriptionExpiresAt")]
        public DateTime? SubscriptionExpiresAt { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName)
            ? "U"
            : FullName.Trim().Substring(0, 1).ToUpperInvariant();

        public string RoleLabel => IsAdmin ? "Admin" : "User";

        public string CreatedDisplay => CreatedAt.ToLocalTime().ToString("dd MMM yyyy");
    }
}