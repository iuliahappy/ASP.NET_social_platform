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

        //[Authorize(Roles = "Registered_User,Administrator")]
        //[HttpPost]
        //public IActionResult PendingResponse(bool response)
        //{
            
        //    return View();
        //}

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

        [HttpPost]
        public IActionResult AddUserToGroup(int groupId, string newUserId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var currentUserRole = _db.GroupRoles
                                    .FirstOrDefault(gr => gr.GroupId == groupId && gr.ApplicationUserId == currentUserId);

            if (currentUserRole == null || currentUserRole.RoleName != "Owner")
            {
                return Forbid();
            }

            var isAlreadyMember = _db.UserConversations
                                    .Any(uc => uc.ApplicationUserId == newUserId && uc.Conversation.GroupId == groupId);

            if (isAlreadyMember) return BadRequest("User is already in the group.");

            var conversation = _db.Conversations
                                    .FirstOrDefault(c => c.GroupId == groupId);

            if (conversation == null) return NotFound();

            _db.UserConversations.Add(new UserConversation
            {
                ApplicationUserId = newUserId,
                ConversationId = conversation.Id,
                LastEntry = DateTime.Now
            });

            _db.GroupRoles.Add(new RoleTable
            {
                ApplicationUserId = newUserId,
                GroupId = groupId,
                RoleName = "Member"
            });

            _db.SaveChanges();

            return RedirectToAction("Show", "Conversations", new { id = conversation.Id });
        }
    }
}
