using System.ComponentModel.DataAnnotations;

namespace AskBofah.Api.Models
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = "New conversation";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;

        // Navigation
        public User? User { get; set; }
        public List<Message> Messages { get; set; } = new();
    }
}