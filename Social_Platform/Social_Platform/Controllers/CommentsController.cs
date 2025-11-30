using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Platform.Data;
using Social_Platform.Models;

namespace Social_Platform.Controllers
{
    public class CommentsController : Controller
    {
        private readonly AppDbContext _db;

        public CommentsController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index(int id)
        {
            var comments = _db.Comments
                            .Where(c => c.PostId == id)
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

            if(ModelState.IsValid)
            {
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
