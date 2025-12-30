using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
