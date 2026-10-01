using System.ComponentModel.DataAnnotations;

namespace AskBofah.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Subscription fields (Paystack)
        public string PlanTier { get; set; } = "Free"; // Free, Pro, Unlimited
        public DateTime? SubscriptionExpiresAt { get; set; }
        public string? PaystackCustomerCode { get; set; }
        public string? PaystackSubscriptionCode { get; set; }

        // Usage tracking (for free-tier limits)
        public int MessagesSentToday { get; set; } = 0;
        public DateTime LastMessageDate { get; set; } = DateTime.UtcNow.Date;

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public List<Conversation> Conversations { get; set; } = new();
    }
}