//using Microsoft.AspNetCore.Mvc;
//using SocialPlatformTime.Data;
//using SocialPlatformTime.Models;
//using System;

//namespace SocialPlatformTime.Controllers
//{
//    public class ApplicationUsersController : Controller
//    {
//        private readonly AppDbContext _db;

//        public ApplicationUsersController(AppDbContext context)
//        {
//            _db = context;
//        }

//        // Show all users
//        public IActionResult Index()
//        {
//            var users = _db.Users.ToList();
//            ViewBag.Users = users;

//            return View();
//        }

//        // Show user profile by id 
//        public IActionResult Show(string id)
//        {
//            var user = _db.Users.Find(id);

//            if (user == null)
//            {
//                return NotFound();
//            }

//            ViewBag.User = user;
//            return View();
//        }

//        // Edit user (view)
//        public IActionResult Edit(string id)
//        {
//            var user = _db.Users.Find(id);

//            if (user == null)
//            {
//                return NotFound();
//            }

//            return View(user);
//        }

//        // Edit user (post request)
//        [HttpPost]
//        public IActionResult Edit(string id, ApplicationUser updatedUser)
//        {
//            var user = _db.Users.Find(id);

//            if (ModelState.IsValid)
//            {
//                user.FirstName = updatedUser.FirstName;
//                user.LastName = updatedUser.LastName;
//                user.PhoneNumber = updatedUser.PhoneNumber;

//                _db.SaveChanges();
//                return Redirect("/ApplicationUsers/Show/" + user.Id);
//            }

//            return View(updatedUser);
//        }
//    }
//}
