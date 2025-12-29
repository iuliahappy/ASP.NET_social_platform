using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment content is required!")]
        public string CommentBody { get; set; }

        public DateTime Date { get; set; }

        public DateTime? EditedDate { get; set; } // data editarii (nullable)

        public int PostId { get; set; } //FK

        public string ApplicationUserId { get; set; } //FK

        public virtual Post? Post { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
