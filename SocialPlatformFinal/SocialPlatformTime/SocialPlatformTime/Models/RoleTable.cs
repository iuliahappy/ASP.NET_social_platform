using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class RoleTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; } 

        public int? GroupId { get; set; }
       
        public string RoleName { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Group? Group { get; set; }

    }
}
