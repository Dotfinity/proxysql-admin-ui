using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Contexts;
using ProxysqlAdminUi.Web.Data;
using ProxysqlAdminUi.Web.Models;
using ProxysqlAdminUi.Web.Security;

namespace ProxysqlAdminUi.Web.Services;


public class DefaultUserSeedService(
    ProxysqlAdminUiWebAuthContext dbContext,
    IConfiguration configuration,
    ILogger<DefaultUserSeedService> logger,
    UserManager<ProxysqlAdminUiWebUser> userManager)
{
    public async Task SeedDefaultUsersAsync()
    {
        try
        {
            await dbContext.Database.EnsureCreatedAsync();
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            var defaultUsers = configuration.GetSection("DefaultUsers")
                .Get<List<DefaultUserModel>>() ?? [];

            var existingUsers = await dbContext.Users.AnyAsync();

            if (!existingUsers)
            {
                foreach (var user in defaultUsers)
                {
                    await CreateDefaultUserAsync(user);
                }
            }

            await FlagUsersStillOnTheirDefaultPasswordAsync(defaultUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding default users");
        }
    }

    private async Task CreateDefaultUserAsync(DefaultUserModel user)
    {
        var result = await userManager.CreateAsync(new ProxysqlAdminUiWebUser
        {
            UserName = user.Username,
            EmailConfirmed = true,
            Email = user.Username + "@localhost"
        }, user.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                logger.LogCritical("Could not create default user {Username}: {ErrorCode}: {ErrorDescription}",
                    user.Username, error.Code, error.Description);
            }

            return;
        }

        await dbContext.SaveChangesAsync();
        var newUser = await userManager.Users.FirstAsync(x => x.UserName == user.Username);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);

        await userManager.ConfirmEmailAsync(newUser, token);

        logger.LogInformation("Added default user: {Username}", user.Username);
    }

    /// <summary>
    /// Marks every configured default account that still authenticates with its configured password,
    /// so <see cref="MustChangePasswordMiddleware"/> forces a change on the next request.
    /// </summary>
    /// <remarks>
    /// This runs on every startup, not just on the run that creates the account: a deployment that
    /// has been live on the documented default password since before this check existed must be
    /// caught too. Once the operator picks a different password the check stops matching and the
    /// claim is never re-added.
    /// </remarks>
    private async Task FlagUsersStillOnTheirDefaultPasswordAsync(List<DefaultUserModel> defaultUsers)
    {
        foreach (var defaultUser in defaultUsers)
        {
            var user = await userManager.FindByNameAsync(defaultUser.Username);

            if (user is null || !await userManager.CheckPasswordAsync(user, defaultUser.Password))
            {
                continue;
            }

            var claims = await userManager.GetClaimsAsync(user);

            if (claims.Any(claim => claim.Type == PaiClaims.MustChangePassword))
            {
                continue;
            }

            var result = await userManager.AddClaimAsync(
                user,
                new Claim(PaiClaims.MustChangePassword, PaiClaims.TrueValue));

            if (result.Succeeded)
            {
                logger.LogWarning(
                    "User {Username} still uses the configured default password and must change it before continuing.",
                    defaultUser.Username);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    logger.LogError("Could not flag {Username} for a password change: {ErrorCode}: {ErrorDescription}",
                        defaultUser.Username, error.Code, error.Description);
                }
            }
        }
    }
}
