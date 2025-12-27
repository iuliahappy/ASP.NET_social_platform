using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int? GroupId { get; set; } //FK

        public virtual Group? Group { get; set; }

        public virtual ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();

        public virtual ICollection<Message>? Messages { get; set; } = new List<Message>();
    }
}
