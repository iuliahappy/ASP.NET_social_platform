using Microsoft.AspNetCore.Mvc;
using Social_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Social_Platform.Data;

namespace Social_Platform.Controllers
{
    public class PostsController : Controller
    {
        private readonly AppDbContext _db;

        public PostsController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var posts = _db.Posts
                             .OrderByDescending(p => p.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        [HttpPost]
        public IActionResult New(Post post)
        {
            post.Date = DateTime.Now;

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
