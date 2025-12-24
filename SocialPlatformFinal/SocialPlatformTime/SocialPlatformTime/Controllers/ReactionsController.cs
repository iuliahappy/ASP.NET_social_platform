using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    public class ReactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db=context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [HttpPost]
        public IActionResult React(Reaction reaction)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var existingReaction = _db.Reactions
                .FirstOrDefault(r => r.PostId == reaction.PostId && r.ApplicationUserId == currentUserId);

            if (existingReaction != null)
            {
                _db.Reactions.Remove(existingReaction);
            }
            else
            {
                reaction.ApplicationUserId = currentUserId;
                _db.Reactions.Add(reaction);
            }

            _db.SaveChanges();

            return RedirectToAction("Show", "Posts", new { id = reaction.PostId });
        }
    }
}
