namespace SocialPlatformTime.Models
{
    public class UserConversation
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int ConversationId { get; set; }
        public DateTime LastEntry { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Conversation Conversation { get; set; }
    }
}