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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string content)
        {
            // Folosim FirstOrDefault pentru a evita eroarea de composite key
            var message = _db.Messages.FirstOrDefault(m => m.Id == id);
            var currentUserId = _userManager.GetUserId(User);

            if (message == null) return NotFound();

            // Verificăm dacă mesajul aparține utilizatorului logat
            if (message.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(content))
            {
                message.Content = content;
                _db.SaveChanges();
            }

            return RedirectToAction("Show", "Conversations", new { id = message.ConversationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Căutăm mesajul
            var message = _db.Messages.FirstOrDefault(m => m.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            // 1. SECURITATE: Verificăm dacă cel care șterge este autorul mesajului
            if (message.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            // Salvăm ID-ul conversației înainte de a șterge obiectul din memorie
            int conversationId = message.ConversationId;

            _db.Messages.Remove(message);
            _db.SaveChanges();

            // 2. REDIRECȚIONARE: Numele controllerului trebuie să fie la plural "Conversations" (după cum ai în Show)
            return RedirectToAction("Show", "Conversations", new { id = conversationId });
        }
    }
}
