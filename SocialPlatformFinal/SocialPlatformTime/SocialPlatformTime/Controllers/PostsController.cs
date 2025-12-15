using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [Authorize(Roles = "User, Editor, Admin")]
        public IActionResult Index()
        {
            var posts = _db.Posts
                             .Include(p => p.User)
                             .OrderByDescending(p => p.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        [Authorize(Roles = "User, Editor, Admin")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "User, Editor, Admin")]
        public IActionResult New(Post post)
        {
            post.Date = DateTime.Now;

            post.UserId = _userManager.GetUserId(User); 

            if (ModelState.IsValid)
            {
                _db.Posts.Add(post);
                _db.SaveChanges();
                TempData["message"] = "The post has been added!";
                return RedirectToAction("Index");
            }

            else
            {
                return View(post);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Post? post = _db.Posts.Find(id);

            if (post is null)
            {
                return NotFound();
            }

            _db.Posts.Remove(post);

            try
            {
                _db.SaveChanges();
                TempData["message"] = "The post has been deleted!";
            }
            catch (DbUpdateException)
            {
                TempData["message"] = "The post cannot be deleted!";
            }

            return RedirectToAction("Index");
        }
    }
}
