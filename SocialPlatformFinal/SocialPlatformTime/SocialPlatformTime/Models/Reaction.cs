using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Reaction
    {
        [Key]
        public int Id { get; set; }

        public int ReactionCode { get; set; } //@ The code of the specific emoji
        //@ I think we can replace the code with the emoji in the view

        public string ApplicationUserId { get; set; } // FK

        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Post? Post { get; set; }

    }
}
