using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime dateTime { get; set; }

        public bool IsRead { get; set; }    

        public bool IsGroupOrNot { get; set; } // @ True if group chat, false if one-on-one chat

        public virtual Group? Group { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
