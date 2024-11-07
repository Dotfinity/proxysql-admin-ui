using Microsoft.AspNetCore.Identity;
using ProxysqlAdminUi.Web.Data;

namespace ProxysqlAdminUi.Web.Components.Account;
internal sealed class IdentityUserAccessor(UserManager<ProxysqlAdminUiWebUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<ProxysqlAdminUiWebUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
