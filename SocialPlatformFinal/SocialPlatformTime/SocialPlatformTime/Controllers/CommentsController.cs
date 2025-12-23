using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace Social_Platform.Controllers
{
    public class CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public IActionResult Index(int id)
        {
            var comments = _db.Comments
                            .Where(c => c.PostId == id)
                            .Include(c => c.ApplicationUser)
                            .OrderByDescending(p => p.Date);

            ViewBag.PostId = id;
            ViewBag.Comments = comments;

            return View();
        }

        // Add a comm for an asociated post
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                comm.ApplicationUserId = _userManager.GetUserId(User);
                _db.Comments.Add(comm);
                _db.SaveChanges();
                return Redirect("/Comments/Index/" + comm.PostId);
            }
            else
            {
                return Redirect("/Comments/Index/" + comm.PostId);
            }
        }


        // Delete a comm from a post
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Comment comm = _db.Comments.Find(id);
            _db.Comments.Remove(comm);
            _db.SaveChanges();
            return Redirect("/Comments/Index/" + comm.PostId);
        }
    }
}
