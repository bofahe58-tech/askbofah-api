using System.ComponentModel.DataAnnotations;

namespace AskBofah.Api.DTOs
{
    public class SendMessageRequest
    {
        public Guid? ConversationId { get; set; }

        [Required, MinLength(1)]
        public string Content { get; set; } = string.Empty;
    }

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
    }

    public class ConversationDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<MessageDto> Messages { get; set; } = new();
    }

    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}