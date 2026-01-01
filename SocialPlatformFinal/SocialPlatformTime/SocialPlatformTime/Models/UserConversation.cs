namespace SocialPlatformTime.Models
{
    public class UserConversation
    {
        public int Id { get; set; }

        public string? ApplicationUserId { get; set; }
        public int? ConversationId { get; set; }
        public DateTime LastEntry { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Conversation? Conversation { get; set; }
    }
}