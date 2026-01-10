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

            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == groupId);

            var isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == groupId &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            if (group == null || (!isOwner && !User.IsInRole("Administrator")))
            {
                return Forbid();
            }

            group.Description = newDescription;
            _db.SaveChanges();

            TempData["messageSend"] = "The group description has been successfully modified!";

            return RedirectToAction("Show", "Conversations", new { id = group.Conversation.Id });
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Administrator");
            bool isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == id &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            var roles = _db.GroupRoles.Where(rt => rt.GroupId == id);
            _db.GroupRoles.RemoveRange(roles);

            if (group.Conversation != null)
            {
                int convId = group.Conversation.Id;

                var messages = _db.Messages.Where(m => m.ConversationId == convId);
                _db.Messages.RemoveRange(messages);

                var userConvs = _db.UserConversations.Where(uc => uc.ConversationId == convId);
                _db.UserConversations.RemoveRange(userConvs);

                _db.Conversations.Remove(group.Conversation);
            }

            _db.Groups.Remove(group);
            _db.SaveChanges();

            TempData["message"] = "The group and all associated data have been deleted.";

            return RedirectToAction("Index", "Conversations");
        }

        [Authorize(Roles = "Registered_User,Administrator")]
        [HttpPost]
        public IActionResult Leave(int id) // id este GroupId
        {
            var currentUserId = _userManager.GetUserId(User);

            // 1. Găsim grupul pentru a identifica conversația asociată
            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            var nrOwners = _db.GroupRoles
                                .Where(gr => gr.RoleName == "Owner" && gr.GroupId == id)
                                .Count();
            var isOwner = _db.GroupRoles
                                .Any(gr => gr.RoleName == "Owner" 
                                && gr.GroupId == id 
                                && gr.ApplicationUserId == currentUserId);

            if (nrOwners == 1 && isOwner)
            {
                return DeleteInternal(group.Id);
            }

            // 2. Ștergem rolul utilizatorului din acest grup
            var userRole = _db.GroupRoles
                              .FirstOrDefault(gr => gr.GroupId == id && gr.ApplicationUserId == currentUserId);

            if (userRole != null)
            {
                _db.GroupRoles.Remove(userRole);
            }

            // 3. Ștergem accesul utilizatorului la conversația grupului
            if (group.Conversation != null)
            {
                var userConv = _db.UserConversations
                                  .FirstOrDefault(uc => uc.ConversationId == group.Conversation.Id && uc.ApplicationUserId == currentUserId);

                if (userConv != null)
                {
                    _db.UserConversations.Remove(userConv);
                }
            }

            _db.SaveChanges();

            TempData["message"] = "You have successfully left the group: " + group.Name;

            return RedirectToAction("Index", "Conversations");
        }

        private IActionResult DeleteInternal(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Administrator");
            bool isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == id &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            var roles = _db.GroupRoles.Where(rt => rt.GroupId == id);
            _db.GroupRoles.RemoveRange(roles);

            if (group.Conversation != null)
            {
                int convId = group.Conversation.Id;

                var messages = _db.Messages.Where(m => m.ConversationId == convId);
                _db.Messages.RemoveRange(messages);

                var userConvs = _db.UserConversations.Where(uc => uc.ConversationId == convId);
                _db.UserConversations.RemoveRange(userConvs);

                _db.Conversations.Remove(group.Conversation);
            }

            _db.Groups.Remove(group);
            _db.SaveChanges();

            TempData["message"] = "The group and all associated data have been deleted.";

            return RedirectToAction("Index", "Conversations");
        }

        [HttpPost]
        [Authorize(Roles = "Registered_User,Administrator")]
        public IActionResult Kick(int groupId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var group = _db.Groups
                           .Include(g => g.Conversation)
                           .FirstOrDefault(g => g.Id == groupId);

            if (group == null) return NotFound();

            bool isAdmin = User.IsInRole("Administrator");
            bool isOwner = _db.GroupRoles.Any(gr =>
                gr.GroupId == groupId &&
                gr.ApplicationUserId == currentUserId &&
                gr.RoleName == "Owner");

            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            if (userId == currentUserId)
            {
                return BadRequest("You cannot kick yourself. Use the Leave option instead.");
            }

            var userRole = _db.GroupRoles
                              .FirstOrDefault(gr => gr.GroupId == groupId && gr.ApplicationUserId == userId);
            if (userRole != null)
            {
                _db.GroupRoles.Remove(userRole);
            }

            if (group.Conversation != null)
            {
                var userConv = _db.UserConversations
                                  .FirstOrDefault(uc => uc.ConversationId == group.Conversation.Id && uc.ApplicationUserId == userId);
                if (userConv != null)
                {
                    _db.UserConversations.Remove(userConv);
                }
            }

            _db.SaveChanges();

            TempData["message"] = "The user has been removed from the group.";

            return RedirectToAction("Show", "Conversations", new { id = group.Conversation.Id });
        }
    }
}
