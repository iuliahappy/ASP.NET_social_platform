using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string UserId { get; set; } // UserId = SenderId
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public DateTime dateTime { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Conversation Conversation { get; set; }
    }
}
