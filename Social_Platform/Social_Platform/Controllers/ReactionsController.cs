using Microsoft.AspNetCore.Mvc;
using Social_Platform.Models;

namespace Social_Platform.Controllers
{
    public class ReactionsController : Controller
    {
        private readonly AppDbContext _db;

        public ReactionsController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
