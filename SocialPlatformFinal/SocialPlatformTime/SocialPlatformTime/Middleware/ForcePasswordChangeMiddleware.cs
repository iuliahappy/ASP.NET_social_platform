using Microsoft.AspNetCore.Identity;
using SocialPlatformTime.Models;

public class ForcePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public ForcePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user != null && user.MustChangePassword)
            {
                var path = context.Request.Path.Value?.ToLower();


                bool allowedPath =
                    path.StartsWith("/applicationusers/changepasswordfirstlogin") ||
                    path.StartsWith("/identity/account/logout") || 
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
                    context.Response.Redirect("/ApplicationUsers/ChangePasswordFirstLogin");
                    return;
                }
            }
        }

        await _next(context);
    }
}
