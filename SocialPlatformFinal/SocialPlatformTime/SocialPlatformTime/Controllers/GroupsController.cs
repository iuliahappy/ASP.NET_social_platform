using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;
using System.Security.Claims;

namespace SocialPlatformTime.Controllers
{
    public class GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext _db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult New(string groupName)
        //{
        //    if (string.IsNullOrWhiteSpace(groupName))
        //    {
        //        return RedirectToAction("Index", "Conversations");
        //    }

        //    var currentUserId = _userManager.GetUserId(User);

        //    var newGroup = new Group
        //    {
        //        Name = groupName,
        //        Description = "",
        //        CreateDGroup = DateTime.Now
        //    };

        //    _db.Groups.Add(newGroup);
        //    _db.SaveChanges();

        //    var groupRole = new RoleTable
        //    {
        //        ApplicationUserId = currentUserId,
        //        RoleName = "Owner",
        //        GroupId = newGroup.Id
        //    };
        //    _db.GroupRoles.Add(groupRole);

        //    var groupConversation = new Conversation
        //    {
        //        GroupId = newGroup.Id
        //    };
        //    _db.Conversations.Add(groupConversation);

        //    _db.UserConversations.Add(new UserConversation
        //    {
        //        ConversationId = groupConversation.Id,
        //        ApplicationUserId = currentUserId,
        //        LastEntry = DateTime.Now
        //    });

        //    _db.SaveChanges();

        //    return RedirectToAction("Show", "Conversations", new { id = groupConversation.Id });
        //}

        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var groupsNotJoined = _db.Groups
                .Where(g => !g.RoleTables.Any(r => r.ApplicationUserId == currentUserId))
                .ToList();

            return View(groupsNotJoined);
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        [HttpPost]
        public IActionResult Join(int id)
        {
            var currentUser = _userManager.GetUserId(User);

            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound("The group wasn't found!");
            }

            var existingRole = _db.GroupRoles
                                 .FirstOrDefault(gr => gr.ApplicationUserId == currentUser && gr.GroupId == id);

            if (existingRole == null)
            {
                string assignedRole = User.IsInRole("Administrator") ? "Owner" : "Pending";

                var newRole = new RoleTable
                {
                    ApplicationUserId = currentUser,
                    GroupId = id,
                    RoleName = assignedRole
                };
                _db.GroupRoles.Add(newRole);

                if (assignedRole == "Owner")
                {
                    if (group.Conversation != null)
                    {
                        var newUserConv = new UserConversation
                        {
                            ApplicationUserId = currentUser,
                            ConversationId = group.Conversation.Id,
                            LastEntry = DateTime.Now
                        };

                        _db.UserConversations.Add(newUserConv);
                    }

                    TempData["messageSend"] = $"As an Administrator, you have been added as Owner of '{group.Name}'.";
                }
                else
                {
                    TempData["messageSend"] = $"Your request to join '{group.Name}' has been sent!";
                }

                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult PendingList()
        {
            var currentUserId = _userManager.GetUserId(User);

            var myOwnedGroupIds = _db.GroupRoles
                .Where(gr => gr.ApplicationUserId == currentUserId && gr.RoleName == "Owner")
                .Select(gr => gr.GroupId)
                .ToList();

            var pendingRequests = _db.GroupRoles
                .Include(gr => gr.Group)
                .Include(gr => gr.ApplicationUser)
                .Where(gr => myOwnedGroupIds.Contains(gr.GroupId) && gr.RoleName == "Pending")
                .ToList()
                .GroupBy(gr => gr.Group.Name);

            return View(pendingRequests);
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        [HttpPost]
        public IActionResult PendingResponse(int requestId, bool response)
        {
            var currentUserId = _userManager.GetUserId(User);

            var request = _db.GroupRoles
                .Include(gr => gr.Group)
                .ThenInclude(g => g.Conversation)
                .FirstOrDefault(gr => gr.Id == requestId);

            if (request == null)
            {
                return NotFound("The request wasn't found!");
            }

            var isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == request.GroupId &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            if (!isOwner)
            {
                return Forbid();
            }

            if (response)
            {
                request.RoleName = "Member";

                if (request.Group?.Conversation != null)
                {
                    var userConv = new UserConversation
                    {
                        ApplicationUserId = request.ApplicationUserId,
                        ConversationId = request.Group.Conversation.Id,
                        LastEntry = DateTime.Now
                    };
                    _db.UserConversations.Add(userConv);
                }
                TempData["messageSend"] = $"The user was accepted in {request.Group.Name}.";
            }
            else
            {
                _db.GroupRoles.Remove(request);
                TempData["messageSend"] = "The request has been declined.";
            }

            _db.SaveChanges();
            return RedirectToAction("PendingList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult New(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return RedirectToAction("Index", "Conversations");
            }

            var currentUserId = _userManager.GetUserId(User);

            var newGroup = new Group
            {
                Name = groupName,
                Description = "",
                CreateDGroup = DateTime.Now
            };
            _db.Groups.Add(newGroup);

            var groupRole = new RoleTable
            {
                ApplicationUserId = currentUserId,
                RoleName = "Owner",
                Group = newGroup
            };
            _db.GroupRoles.Add(groupRole);

            var groupConversation = new Conversation
            {
                Group = newGroup
            };
            _db.Conversations.Add(groupConversation);

            _db.UserConversations.Add(new UserConversation
            {
                Conversation = groupConversation,
                ApplicationUserId = currentUserId,
                LastEntry = DateTime.Now
            });

            _db.SaveChanges();

            return RedirectToAction("Show", "Conversations", new { id = groupConversation.Id });
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        [HttpPost]
        public IActionResult ModifyDescription(int groupId, string newDescription)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Adăugăm .Include(g => g.Conversation) pentru a evita NullReferenceException
            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == groupId);

            var isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == groupId &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            // Verificăm dacă grupul există și dacă este owner sau admin
            if (group == null || (!isOwner && !User.IsInRole("Administrator")))
            {
                return Forbid();
            }

            group.Description = newDescription;
            _db.SaveChanges();

            TempData["messageSend"] = "The group description has been successfully modified!";

            // Acum group.Conversation nu mai este null
            return RedirectToAction("Show", "Conversations", new { id = group.Conversation.Id });
        }
    }
}
