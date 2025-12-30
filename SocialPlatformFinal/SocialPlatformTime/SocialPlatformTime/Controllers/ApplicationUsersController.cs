using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Controllers
{
    // Only App Admins are allowed to perform CRUD operations on Users
    [Authorize(Roles = "Administrator")]
    public class ApplicationUsersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager
    ) : Controller
    {
        // For CRUD Operations on Users from Database Data
        private readonly ApplicationDbContext _db = context;
        // For Identity FrameWork User Management (more ocmplex User Profile stuff, like Identity consistency, password hashing etc)
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        // For authentification, login/logout, credential validation etc
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        // Display all users
        public IActionResult Index()
        {
            // Fetch user roles also
            var users = _db.Users.Include(u => u.RoleTables).ToList();
            ViewBag.Users = users;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        // Display only user profile by requested id 
        public IActionResult Show(string id)
        {
            var user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.User = user;
            return View();
        }

        // Create User (get request)
        public IActionResult Create()
        {
            // Dropdown which lists all available roles
            ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            return View();
        }

        // Create User (post request)
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser newUser, string selectedRole, string password)
        {
            if (!ModelState.IsValid)
            {
                // Reload roles for dropdown in case of error
                ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();
                // 'Re-try' creation of new user
                return View(newUser);
            }

            // If data is valid, then create Identity user
            var result = await _userManager.CreateAsync(newUser, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                // Retry user creation
                ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

                return View(newUser);
            }

            // Role assignment
            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.AddToRoleAsync(newUser, selectedRole);
            }

            TempData["message"] = "User created successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        // Edit user (get request)
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync(); // Load roles first 

            var roleItems = new List<SelectListItem>();
            foreach (var role in roles)
            {
                bool isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                roleItems.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name,
                    Selected = isInRole
                });
            }

            ViewBag.Roles = roleItems;

            return View(user);
        }

        // Edit user (post request)
        [HttpPost]
        public async Task<IActionResult> Edit(string id, ApplicationUser updatedUser, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();
                return View(updatedUser);
            }

            // Update user details
            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.PhoneNumber = updatedUser.PhoneNumber;

            await _userManager.UpdateAsync(user);

            // Update role
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, selectedRole);
            }

            TempData["message"] = "User profile updated!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", new { id = user.Id });
        }

        // Delete user (post request)
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            // Retrieve User
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Delete 
            await _userManager.DeleteAsync(user);

            // Confirm succesful deletion
            TempData["message"] = "User deleted successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        //// Login procedure (get request)
        //[AllowAnonymous]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //// Login procedure (post request)
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> Login(string email, string password)
        //{
        //    var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
        //    if (result.Succeeded)
        //        return RedirectToAction("Index", "Home");

        //    ModelState.AddModelError("", "Invalid login attempt");
        //    return View();
        //}

        //// Logout procdure (post only)
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction("Login");
        //}

        //// Password Change (get request)
        //[AllowAnonymous]
        //public IActionResult ChangePassword()
        //{
        //    return View();
        //}

        //// Password Change (post request)
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> ChangePassword(string id, string newPassword)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user == null) return NotFound();

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        //    if (result.Succeeded)
        //    {
        //        TempData["message"] = "Password changed successfully!";
        //        TempData["messageType"] = "alert-success";
        //    }
        //    else
        //    {
        //        foreach (var error in result.Errors)
        //            ModelState.AddModelError("", error.Description);
        //    }

        //    return RedirectToAction("Edit", new { id });
        //}
    }
}
