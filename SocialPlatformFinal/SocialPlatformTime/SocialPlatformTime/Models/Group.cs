using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDGroup { get; set; }
        public virtual Conversation? Conversation { get; set; }
        public virtual ICollection<RoleTable>? RoleTables { get; set; } = [];
    }
}
