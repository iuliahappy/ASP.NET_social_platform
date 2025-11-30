using Microsoft.AspNetCore.Mvc;
using Social_Platform.Data;

namespace Social_Platform.Controllers
{
    public class ReactionsController : Controller
    {
        private readonly AppDbContext _db;

        public ReactionsController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult React()
        {
            return View();
        }
    }
}
