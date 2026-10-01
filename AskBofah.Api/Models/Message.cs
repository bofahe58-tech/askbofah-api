using System.ComponentModel.DataAnnotations;

namespace AskBofah.Api.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ConversationId { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; } = "user"; // "user" or "assistant"

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? TokensUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Conversation? Conversation { get; set; }
    }
}