using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string PostDescription { get; set; }

        public string ImageURL { get; set; } //@ The path for the photo in wwwroot

        public DateTime Date { get; set; }

        public string UserId { get; set; } //FK


        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Reaction>? Reactions { get; set; } = new List<Reaction>();
    }
}
