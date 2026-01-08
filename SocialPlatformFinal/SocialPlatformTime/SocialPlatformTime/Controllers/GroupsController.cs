using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using SocialPlatformTime.Data;
using SocialPlatformTime.Models;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}
