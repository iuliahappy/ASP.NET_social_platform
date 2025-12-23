using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class RoleTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } // FK

        public int GroupId { get; set; } // FK

        public string RoleName { get; set; } 

        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual Group? Group { get; set; }

    }
}
