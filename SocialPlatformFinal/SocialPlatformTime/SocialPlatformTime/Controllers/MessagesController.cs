using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Controllers
{
    public class MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [HttpPost]
        public IActionResult New([Bind("Content,ConversationId")] Message message)
        {
            var currentUserId = _userManager.GetUserId(User);

            var permitted = _db.UserConversations
                               .Any(uc => uc.ConversationId == message.ConversationId && uc.ApplicationUserId == currentUserId);

            if (permitted)
            {
                message.dateTime = DateTime.Now;
                message.IsRead = false;
                message.ApplicationUserId = currentUserId;

                _db.Messages.Add(message);
                _db.SaveChanges();

                return RedirectToAction("Show", "Conversations", new { id = message.ConversationId });
            }
            
            return Forbid();
        }
    }
}
