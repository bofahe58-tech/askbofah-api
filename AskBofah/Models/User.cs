using Newtonsoft.Json;

namespace AskBofah.Models
{
    public class User
    {
        [JsonProperty("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "student";

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}