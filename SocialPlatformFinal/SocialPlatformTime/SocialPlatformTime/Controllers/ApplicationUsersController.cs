using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;
using System.Globalization;

namespace SocialPlatformTime.Controllers
{
    [Authorize(Roles = "Registered_User,Administrator")]
    public class ApplicationUsersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager
    ) : Controller
    {
        // For CRUD Operations on Users from Database Data
        private readonly ApplicationDbContext _db = context;
        // For Identity FrameWork User Management (more complex User Profile stuff, like Identity consistency, password hashing etc)
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        // For authentification, login/logout, credential validation etc
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [Authorize(Roles = "Administrator")]
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
        [AllowAnonymous]
        public async Task<IActionResult> Show(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var currUserId = _userManager.GetUserId(User);

            var userWithPosts = await _userManager.Users
                 .Include(u => u.Posts)
                 .ThenInclude(p => p.Reactions)
                 .FirstOrDefaultAsync(u => u.Id == id);

            if (userWithPosts == null)
                return NotFound();
            // Non-Admin user tries to view Admin User Profile
            else if (!User.IsInRole("Administrator") && await _userManager.IsInRoleAsync(userWithPosts, "Administrator"))
                return Forbid();

           
            // Can View <=> Profile is Public, is Owner of Profile, current User is an Admin SAU are follow acceptat
            bool isAcceptedFollower = false;
            if (currUserId != null)
            {
                isAcceptedFollower = _db.FollowRequests.Any(fr =>
                    fr.FollowerId == currUserId &&
                    fr.FollowingId == id &&
                    fr.Status == "accepted");
            }

            bool canView = userWithPosts.IsPublic || (currUserId == id) || User.IsInRole("Administrator") || isAcceptedFollower;

            ViewBag.CanViewFullProfile = canView;

            bool following = false; // whether they are following the current user
            bool followed = false; // whether the current user follows them

            // Admin users cannot be Followed or Follow Other Users
            bool isEitherUserAdmin = User.IsInRole("Administrator") || await _userManager.IsInRoleAsync(userWithPosts, "Administrator");

            if (!isEitherUserAdmin)
            { 
                following = _db.FollowRequests.Any(fr =>
                    fr.FollowerId == currUserId &&
                    fr.FollowingId == id &&
                    fr.Status == "accepted");

                followed = _db.FollowRequests.Any(fr =>
                    fr.FollowerId == id &&
                    fr.FollowingId == currUserId &&
                    fr.Status == "accepted");
            }

            ViewBag.User = userWithPosts;
            ViewBag.IFollow = following;
            ViewBag.FollowsMe = followed;
            ViewBag.IsCurrentUser = currUserId == id;
            ViewBag.ShowFollowSystem = !isEitherUserAdmin;

            if (canView)
            {
                var followersList = _db.FollowRequests
                    .Where(fr => fr.FollowingId == id && fr.Status == "accepted")
                    .Select(fr => fr.Follower)
                    .ToList();

                var followingList = _db.FollowRequests
                    .Where(fr => fr.FollowerId == id && fr.Status == "accepted")
                    .Select(fr => fr.Following)
                    .ToList();

                ViewBag.FollowersList = followersList;
                ViewBag.FollowingList = followingList;
            }
            else
            {
                // Private profile and not following, not allowed → only counts
                ViewBag.FollowersCount = _db.FollowRequests
                    .Count(fr => fr.FollowingId == userWithPosts.Id && fr.Status == "accepted");
                ViewBag.FollowingCount = _db.FollowRequests
                    .Count(fr => fr.FollowerId == userWithPosts.Id && fr.Status == "accepted");

                ViewBag.FollowersList = new List<ApplicationUser>();
                ViewBag.FollowingList = new List<ApplicationUser>();
            }

            // Cereri de follow PENDING primite de utilizatorul curent
            if (currUserId == id) // doar pe propriul profil
            {
                var pendingRequests = _db.FollowRequests
                    .Where(fr => fr.FollowingId == currUserId && fr.Status == "pending")
                    .Include(fr => fr.Follower)
                    .ToList();

                ViewBag.PendingRequests = pendingRequests;
            }

            // Setează SavedPostIds pentru utilizatorul curent
            if (User.Identity?.IsAuthenticated == true && currUserId != null)
            {
                var savedPostIds = _db.SavedPosts
                    .Where(sp => sp.ApplicationUserId == currUserId)
                    .Select(sp => sp.PostId)
                    .ToList();
                ViewBag.SavedPostIds = savedPostIds;
            }
            else
            {
                ViewBag.SavedPostIds = new List<int>();
            }

            return View();
        }
        
        [Authorize(Roles = "Administrator")]
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

        [Authorize(Roles = "Administrator")]
        // Create User (post request)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser newUser, string selectedRole)
        {
            var emailValidator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            bool emailValid = emailValidator.IsValid(newUser.Email);

            bool isInvalid =
                string.IsNullOrWhiteSpace(newUser.Email) || !emailValid ||
                string.IsNullOrWhiteSpace(newUser.FirstName) ||
                string.IsNullOrWhiteSpace(newUser.LastName) ||
                string.IsNullOrWhiteSpace(newUser.ProfileDescription) ||
                newUser.ImageFile == null;

            if (isInvalid) // no model state because it (Identity) expects required fields we dont use
            {
                TempData["message"] = "Please complete all of the profile fields in order to continue.";
                TempData["messageType"] = "alert-danger";
                if (!emailValid)
                    TempData["message"] = "Please enter a valid email address.";

                // Reload roles for dropdown in case of error
                ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();
                // 'Re-try' creation of new user
                return View(newUser);
            }

            // Identity requires UserName → we map it to Email
            newUser.UserName = newUser.Email;

            // Save uploaded image
            var fileName = Guid.NewGuid() + Path.GetExtension(newUser.ImageFile.FileName);
            var filePath = Path.Combine("wwwroot/images", fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await newUser.ImageFile.CopyToAsync(stream);
            newUser.Image = "/images/" + fileName;

            // Generate temp password for initial login then we enforce password change for security 
            string tempPassword = "Temp@" + Guid.NewGuid().ToString("N")[..8];
            newUser.MustChangePassword = true;

            // If data is valid, then create Identity user
            var result = await _userManager.CreateAsync(newUser, tempPassword);

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
                await _userManager.AddToRoleAsync(newUser, selectedRole);

            TempData["message"] = $"User created successfully! Temporary password: {tempPassword}";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ChangePasswordFirstLogin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!user.MustChangePassword)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            string newTempPassword = "Temp@" + Guid.NewGuid().ToString("N")[..8];

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newTempPassword);

            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);

            TempData["message"] = $"Password reset! New temporary password: <strong>{newTempPassword}</strong>";
            TempData["messageType"] = "alert-warning";

            return RedirectToAction("Edit", new { id = user.Id });
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordFirstLogin(string oldPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View();
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            TempData["message"] = "Password updated successfully!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View();
            }

            TempData["message"] = "Password changed successfully!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index", "Home");
        }

        // Edit user (get request)
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.CanChangeRole = false;
            bool isAdmin = User.IsInRole("Administrator");
            bool isSelf = id == _userManager.GetUserId(User);

            if (!isAdmin && !isSelf) // Not Admin and not editing own profile -> invalid operation
                return Forbid();
            else if (isAdmin)// Admin Workflow
                {
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

                    ViewBag.CanChangeRole = true;
                    ViewBag.Roles = roleItems;
                }
            ViewBag.CurrentRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault(); 
            return View(user);
        }

        // Edit user (post request)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser updatedUser, string selectedRole)
        {
            //Console.WriteLine("MEOW");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            bool isAdmin = User.IsInRole("Administrator");
            bool isSelf = (id == _userManager.GetUserId(User));

            if (!isAdmin && !isSelf)
                return Forbid();

            ViewBag.CanChangeRole = false;

            // all required fields are filled 
            bool noPFP = updatedUser.ImageFile == null && string.IsNullOrEmpty(user.Image);
            bool isInvalid =
                string.IsNullOrWhiteSpace(updatedUser.FirstName) ||
                string.IsNullOrWhiteSpace(updatedUser.LastName) ||
                string.IsNullOrWhiteSpace(updatedUser.ProfileDescription) ||
                noPFP;

            if (!ModelState.IsValid || isInvalid)
            {
                TempData["message"] = "Please complete all of the profile fields in order to continue.";
                TempData["messageType"] = "alert-danger";

                if (isAdmin)
                {
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

                    ViewBag.CanChangeRole = true;
                    ViewBag.Roles = roleItems;
                }
                return View(updatedUser);
            }

            // Update basic user details
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.ProfileDescription = updatedUser.ProfileDescription;
            user.IsPublic = updatedUser.IsPublic;
            
            // Update email and (identity) username
            if (user.Email != updatedUser.Email)
            {
                await _userManager.SetEmailAsync(user, updatedUser.Email);
                user.UserName = updatedUser.Email;
            }
            // Update pfp 
            if (updatedUser.ImageFile != null)
            {

                var fileName = Guid.NewGuid() + Path.GetExtension(updatedUser.ImageFile.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updatedUser.ImageFile.CopyToAsync(stream);
                }

                user.Image = "/images/" + fileName;
            }

            // Update user in Identity
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(updatedUser);
            }

            // Update role if admin
            if (User.IsInRole("Administrator"))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!string.IsNullOrEmpty(selectedRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                    await _userManager.AddToRoleAsync(user, selectedRole);
                }
            }

            TempData["message"] = "User profile updated!";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Show", new { id = user.Id });
        }

        // Delete user (post request)
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            // Retrieve User
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Delete 
            await _userManager.DeleteAsync(user);

            // Confirm successful deletion
            TempData["message"] = "User deleted successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<ApplicationUser>());

            var users = _db.Users
                .Where(u =>
                    u.FirstName.Contains(query) ||
                    u.LastName.Contains(query) ||
                    (u.FirstName + " " + u.LastName).Contains(query))
            .ToList();

            return View(users);
        }

        public async Task<IActionResult> CompleteProfile()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // If (somehow ?) already completed, skip
            if (user.IsProfileComplete)
                return RedirectToAction("Index", "Home");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            bool isInvalid =
                string.IsNullOrWhiteSpace(model.FirstName) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.ProfileDescription) ||
                model.ImageFile == null;

            if (!ModelState.IsValid || isInvalid)
            {
                TempData["message"] = "Please complete all of the profile fields in order to continue.";
                TempData["messageType"] = "alert-danger";
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfileDescription = model.ProfileDescription;
            user.IsPublic = model.IsPublic;

            if (model.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                user.Image = "/images/" + fileName;
            }

            await _userManager.UpdateAsync(user);
            
            TempData["message"] = "Profile completed successfully!";
            TempData["messageType"] = "alert-success";
            
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string id)
        {
            var currUserId = _userManager.GetUserId(User);

            if (currUserId == null || id == null || currUserId == id)
            {
                return RedirectToAction("Show", new { id });
            }

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Administrator") || await _userManager.IsInRoleAsync(targetUser, "Administrator")) // ADmins cannot participate in follow flow 
                return Forbid(); // we enfore this again (extra extra sure) 

            var existing = _db.FollowRequests
                .FirstOrDefault(fr => fr.FollowerId == currUserId && fr.FollowingId == id);

            // Profil PUBLIC => follow direct (accepted) / unfollow
            if (targetUser.IsPublic)
            {
                if (existing == null)
                {
                    // Follow direct
                    var fr = new FollowRequest
                    {
                        FollowerId = currUserId,
                        FollowingId = id,
                        Status = "accepted"
                    };
                    _db.FollowRequests.Add(fr);
                    TempData["message"] = "You are now following this user.";
                    TempData["messageType"] = "alert-success";
                }
                else if (existing.Status == "accepted")
                {
                    // Unfollow
                    _db.FollowRequests.Remove(existing);
                    TempData["message"] = "You have unfollowed this user.";
                    TempData["messageType"] = "alert-info";
                }
                else
                {
                    // Orice alt status -> îl facem accepted
                    existing.Status = "accepted";
                    TempData["message"] = "You are now following this user.";
                    TempData["messageType"] = "alert-success";
                }
            }
            else
            {
                // Profil PRIVAT => Pending până la acceptare
                if (existing == null)
                {
                    var fr = new FollowRequest
                    {
                        FollowerId = currUserId,
                        FollowingId = id,
                        Status = "pending"
                    };
                    _db.FollowRequests.Add(fr);
                    TempData["message"] = "Follow request sent.";
                    TempData["messageType"] = "alert-info";
                }
                else if (existing.Status == "pending")
                {
                    _db.FollowRequests.Remove(existing);
                    TempData["message"] = "Follow request cancelled.";
                    TempData["messageType"] = "alert-warning";
                }
                else if (existing.Status == "accepted")
                {
                    // Unfollow
                    _db.FollowRequests.Remove(existing);
                    TempData["message"] = "You have unfollowed this user.";
                    TempData["messageType"] = "alert-info";
                }
                else
                {
                    // rejected sau altceva -> recreăm pending
                    existing.Status = "pending";
                    TempData["message"] = "Follow request sent again.";
                    TempData["messageType"] = "alert-info";
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Show", new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondFollow(int requestId, string decision)
        {
            var currUserId = _userManager.GetUserId(User);

            var request = _db.FollowRequests
                .Include(fr => fr.Follower)
                .FirstOrDefault(fr => fr.Id == requestId);

            if (request == null)
                return NotFound();

            if (decision != "accept" && decision != "reject") 
                return BadRequest();

            // Doar utilizatorul care PRIMEȘTE cererea poate decide
            if (request.FollowingId != currUserId)
                return Forbid();

            if (decision == "accept")
            {
                request.Status = "accepted";
                TempData["message"] = $"You accepted {request.Follower.UserName}'s follow request.";
                TempData["messageType"] = "alert-success";
            }
            else if (decision == "reject")
            {
                // fie ștergem cererea, fie setam Status = "rejected"
                request.Status = "rejected";
                TempData["message"] = $"You rejected {request.Follower.UserName}'s follow request.";
                TempData["messageType"] = "alert-warning";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Show", new { id = currUserId });
        }

    }
}
