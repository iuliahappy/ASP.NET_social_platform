using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<FollowRequest>? FollowRequestsSent { get; set; }

        public virtual ICollection<FollowRequest>? FollowRequestsReceived { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

        public virtual ICollection<Reaction>? Reactions { get; set; }

        public virtual ICollection<RoleTable>? RoleTables { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }

        public virtual ICollection<SavedPost>? SavedPosts { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileDescription { get; set; }
        public string? Image { get; set; } 
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public bool IsPublic { get; set; } = true; // by default, all profiles start as public

        // To enforce all required fields after having registered using Identity Framework
        //public bool IsProfileComplete =>
        //    !string.IsNullOrWhiteSpace(FirstName) &&
        //    !string.IsNullOrWhiteSpace(LastName) &&
        //    !string.IsNullOrWhiteSpace(ProfileDescription) &&
        //    !string.IsNullOrWhiteSpace(Image);
        public bool IsProfileComplete =>
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(ProfileDescription);
       
        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}