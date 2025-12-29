using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Controllers
{
    public class ConversationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var conversations = _db.Conversations
                .Include(c => c.UserConversations)
                    .ThenInclude(uc => uc.User)
                .Include(c => c.Messages)
                .Where(c => c.UserConversations.Any(uc => uc.UserId == currentUserId))
                .ToList();

            return View(conversations);
        }

        public IActionResult Show(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var messages = _db.Messages
                                .Include(m => m.ApplicationUser) 
                                .Where(m => m.ConversationId == id)
                                .Where(m => m.Conversation.UserConversations.Any(uc => uc.UserId == currentUserId))
                                .OrderBy(m => m.dateTime)
                                .ToList();

            //To put a different color to the user loged
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ConversationId = id;

            return View(messages);
        }
    }
}
