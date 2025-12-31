using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class SavedPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? ApplicationUserId { get; set; } // FK
        public int? PostId { get; set; } // FK

        public DateTime SavedDate { get; set; } // Data când a fost salvată postarea

        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Post? Post { get; set; }
    }
}