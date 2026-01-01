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
                    .ThenInclude(uc => uc.ApplicationUser)
                .Include(c => c.Messages)
                .Where(c => c.UserConversations.Any(uc => uc.ApplicationUserId == currentUserId))
                .ToList();

            return View(conversations);
        }

        //public IActionResult Show(int id)
        //{
        //    var currentUserId = _userManager.GetUserId(User);

        //    var messages = _db.Messages
        //                        .Include(m => m.ApplicationUser) 
        //                        .Where(m => m.ConversationId == id)
        //                        .Where(m => m.Conversation.UserConversations.Any(uc => uc.UserId == currentUserId))
        //                        .OrderBy(m => m.dateTime)
        //                        .ToList();

        //    //To put a different color to the user loged
        //    ViewBag.CurrentUserId = currentUserId;
        //    ViewBag.ConversationId = id;

        //    return View(messages);
        //}

        public IActionResult Show(int id)
        {
            if(_db.Conversations.Find(id) == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            // Update LastEntry
            var userEntry = _db.UserConversations
                               .FirstOrDefault(uc => uc.ConversationId == id && uc.ApplicationUserId == currentUserId);

            if (userEntry != null)
            {
                userEntry.LastEntry = DateTime.Now;
                _db.SaveChanges();
            }

            // Extract the messages of the conversation
            var messages = _db.Messages
                                .Include(m => m.ApplicationUser)
                                .Where(m => m.ConversationId == id)
                                .Where(m => m.Conversation.UserConversations.Any(uc => uc.ApplicationUserId == currentUserId))
                                .OrderBy(m => m.dateTime)
                                .ToList();

            // Update the "seen"
            var otherUsersLastEntries = _db.UserConversations
                                            .Where(uc => uc.ConversationId == id && uc.ApplicationUserId != currentUserId)
                                            .Select(uc => uc.LastEntry)
                                            .ToList();

            foreach (var msg in messages)
            {
                if (otherUsersLastEntries.Any() && otherUsersLastEntries.All(le => le >= msg.dateTime))
                {
                    msg.IsRead = true;
                }
                else
                {
                    msg.IsRead = false;
                }
            }

            _db.SaveChanges();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ConversationId = id;

            return View(messages);
        }

        public IActionResult New(int reciverId)
        {
            var currentUser = _userManager.GetUserId(User);
            return RedirectToAction("Conversations", "Show");
        }
    }
}
