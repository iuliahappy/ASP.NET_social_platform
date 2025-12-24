using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string? PostDescription { get; set; }

        public string? TextContent { get; set; }

        public string? Image { get; set; } //@ The path for the photo in wwwroot

        public string? Video { get; set; } //@ The path for the video in wwwroot

        public DateTime Date { get; set; }

        public string ApplicationUserId { get; set; } //FK

        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Reaction>? Reactions { get; set; } = new List<Reaction>();
    }
}
