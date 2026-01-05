using Microsoft.AspNetCore.Identity;
using SocialPlatformTime.Models;

namespace SocialPlatformTime.Middleware
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.CodeAnalysis.Elfie.Serialization;
    using SocialPlatformTime.Models;

    public class ProfileCompletionMiddleware
    {
        private readonly RequestDelegate _next;

        public ProfileCompletionMiddleware(RequestDelegate next)
        {
            _next = next;
        } // to store the next middleware in the platform pipeline

        public async Task InvokeAsync(
                HttpContext context,
                UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                // if nonadmin user profile isnt complete -> we redirect the user to profile edit page
                if (user != null && (await userManager.IsInRoleAsync(user, "Administrator")) == false && !user.IsProfileComplete)
                {
                    var path = context.Request.Path.Value?.ToLower();
                   
                    if (string.IsNullOrEmpty(path))
                    {
                        await _next(context);
                        return;
                    }

                    bool allowedPath =
                        path.StartsWith("/applicationusers/completeprofile") ||
                        path.StartsWith("/applicationusers/edit") ||
                        path.StartsWith("/identity") ||
                        path.StartsWith("/account/logout") ||
                        path.StartsWith("/css") ||
                        path.StartsWith("/js") ||
                        path.StartsWith("/lib") ||
                        path.StartsWith("/images") ||
                        path.StartsWith("/favicon") ||
                        path.StartsWith("/_framework");

                    if (!allowedPath)
                    {
                        context.Response.Redirect("/ApplicationUsers/CompleteProfile");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
