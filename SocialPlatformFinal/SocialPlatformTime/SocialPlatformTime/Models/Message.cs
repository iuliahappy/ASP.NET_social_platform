using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } // UserId = SenderId //FK
        public int ConversationId { get; set; } // FK
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime dateTime { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Conversation? Conversation { get; set; }
    }
}
