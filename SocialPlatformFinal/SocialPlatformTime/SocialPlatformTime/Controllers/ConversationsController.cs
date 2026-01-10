using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Controllers
{
    [Authorize] // Only logged-in users ought to be able to take part in Convo flow
    public class ConversationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Challenge();

            var conversations = _db.Conversations
                .Include(c => c.Group)
                .Include(c => c.UserConversations)
                    .ThenInclude(uc => uc.ApplicationUser)
                .Include(c => c.Messages)
                .Where(c => c.UserConversations.Any(uc => uc.ApplicationUserId == currentUserId))
                .ToList();

            return View(conversations);
        }

        public IActionResult Show(int id)
        {
            if(_db.Conversations.Find(id) == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            var userEntry = _db.UserConversations
                               .FirstOrDefault(uc => uc.ConversationId == id && uc.ApplicationUserId == currentUserId);

            if (userEntry != null)
            {
                userEntry.LastEntry = DateTime.Now;
                _db.SaveChanges();
            }

            var group = _db.Groups
               .Include(g => g.Conversation)
               .FirstOrDefault(g => g.Conversation != null && g.Conversation.Id == id);
            if (group != null)
            {
                ViewBag.GroupId = group.Id;
                ViewBag.GroupDescription = group.Description;

                ViewBag.IsOwner = _db.GroupRoles.Any(gr =>
                    gr.GroupId == group.Id &&
                    gr.ApplicationUserId == currentUserId &&
                    gr.RoleName == "Owner");
            }

            var messages = _db.Messages
                                .Include(m => m.ApplicationUser)
                                .Where(m => m.ConversationId == id)
                                .Where(m => m.Conversation.UserConversations.Any(uc => uc.ApplicationUserId == currentUserId))
                                .OrderBy(m => m.dateTime)
                                .ToList();

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

        [HttpPost]
        public IActionResult New(string receiverId)
        {
            Console.WriteLine(receiverId);
            if (string.IsNullOrEmpty(receiverId)) return BadRequest();

            var currentUserId = _userManager.GetUserId(User);

            var conversationId = _db.UserConversations
                                    .Where(uc => (uc.ApplicationUserId == currentUserId || uc.ApplicationUserId == receiverId) && uc.Conversation.GroupId==null)
                                    .GroupBy(uc => uc.ConversationId)
                                    .Where(g => g.Count() == 2)
                                    .Select(g => g.Key)
                                    .FirstOrDefault();

            if (conversationId == null)
            {
                var newConversation = new Conversation { GroupId = null };
                _db.Conversations.Add(newConversation);
                _db.SaveChanges();

                conversationId = newConversation.Id;

                _db.UserConversations.Add(new UserConversation
                {
                    ConversationId = conversationId,
                    ApplicationUserId = receiverId,
                    LastEntry = DateTime.Now
                });
                _db.UserConversations.Add(new UserConversation
                {
                    ConversationId = conversationId,
                    ApplicationUserId = currentUserId,
                    LastEntry = DateTime.Now
                });

                _db.SaveChanges();
            }

            return RedirectToAction("Show", "Conversations", new { id = conversationId });
        }
    }
}
