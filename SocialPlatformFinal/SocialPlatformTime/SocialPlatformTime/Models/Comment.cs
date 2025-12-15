using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string CommentBody { get; set; }

        public DateTime Date { get; set; }

        public int PostId { get; set; } //FK

        public string? UserId { get; set; } //FK

        public virtual Post? Post { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
