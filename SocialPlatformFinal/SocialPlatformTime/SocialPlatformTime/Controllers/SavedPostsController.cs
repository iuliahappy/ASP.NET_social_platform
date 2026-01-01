using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    [Authorize(Roles = "Registered_User,Administrator")]
    public class SavedPostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        // Toggle Save/Unsave pentru o postare
        [HttpPost]
        public async Task<IActionResult> ToggleSave(int postId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return Json(new { success = false, message = "Please log in" });
            }

            var existingSave = await _db.SavedPosts
                .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.ApplicationUserId == currentUserId);

            bool isSaved;

            if (existingSave != null)
            {
                // Unsave
                _db.SavedPosts.Remove(existingSave);
                isSaved = false;
            }
            else
            {
                // Save
                var savedPost = new SavedPost
                {
                    PostId = postId,
                    ApplicationUserId = currentUserId,
                    SavedDate = DateTime.Now
                };
                _db.SavedPosts.Add(savedPost);
                isSaved = true;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, isSaved = isSaved });
        }

        // Afișează postările salvate ale utilizatorului curent
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var savedPosts = await _db.SavedPosts
                .Where(sp => sp.ApplicationUserId == currentUserId)
                .Include(sp => sp.Post)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(sp => sp.Post)
                    .ThenInclude(p => p.Reactions)
                .OrderByDescending(sp => sp.SavedDate)
                .Select(sp => sp.Post)
                .ToListAsync();


            // Setează SavedPostIds pentru utilizatorul curent
            var savedPostIds = await _db.SavedPosts
                .Where(sp => sp.ApplicationUserId == currentUserId)
                .Select(sp => sp.PostId)
                .ToListAsync();

            ViewBag.SavedPostsIds = savedPostIds;
            ViewBag.SavedPosts = savedPosts;

            return View();
        }
    }
}