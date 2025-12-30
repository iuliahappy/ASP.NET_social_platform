//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;


        [HttpPost]
        public IActionResult React(int postId, string reactionType)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) 
            { 
                return Json(new { success = false, message = "Please log in to react" });
            }

            var existingReaction = _db.Reactions
               .FirstOrDefault(r => r.PostId == postId && r.ApplicationUserId == currentUserId); // Only one Reaction type per User for each Post

            bool hasReaction;

            if (existingReaction != null)
            {
                if (existingReaction.ReactionType == reactionType) // Same reactionn -> untoggle it
                {
                    _db.Reactions.Remove(existingReaction);
                    hasReaction = false;
                }
                else // Change Reaction Type
                {
                    existingReaction.ReactionType = reactionType;
                    hasReaction = true;
                }
            }
            else
            {
                // First Reaction
                _db.Reactions.Add(new Reaction
                {
                    PostId = postId,
                    ApplicationUserId = currentUserId,
                    ReactionType = reactionType
                });
                
                hasReaction = true;
            }

            _db.SaveChanges();

            var reactionCounts = _db.Reactions
                .Where(r => r.PostId == postId)
                .GroupBy(r => r.ReactionType)
                .Select(g => new
                    {
                        ReactionType = g.Key,
                        Count = g.Count()
                    })
                    .ToList(); // Count of reactions by type 

            var allReactionTypes = new[] { "Like", "Love", "Laugh", "Angry" };

            //var reactionSummary = allReactionTypes
            //    .Select(type => new
            //    {
            //        ReactionType = type,
            //        Count = reactionCounts
            //            .FirstOrDefault(r => r.ReactionType == type)?.Count ?? 0
            //    });

            //// Numără totalul de like-uri pentru acest post
            //var totalLikes = _db.Reactions.Count(r => r.PostId == postId);

            return Json(new
            {
                success = true,
                hasReaction = hasReaction
            });
        }
    }
}