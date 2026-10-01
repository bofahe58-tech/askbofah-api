using System.Text.Json.Serialization;

namespace AskBofah.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("planTier")]
        public string PlanTier { get; set; } = "Free";

        [JsonPropertyName("subscriptionExpiresAt")]
        public DateTime? SubscriptionExpiresAt { get; set; }

        [JsonPropertyName("messagesSentToday")]
        public int MessagesSentToday { get; set; }

        [JsonPropertyName("dailyMessageLimit")]
        public int DailyMessageLimit { get; set; } = 50;
    }
}