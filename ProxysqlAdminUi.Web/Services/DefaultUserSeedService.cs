using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; 
using ProxysqlAdminUi.Web.Data;
using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.Services;

 
public class DefaultUserSeedService
{
    private readonly ProxysqlAdminUiWebAuthContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DefaultUserSeedService> _logger;
    private readonly UserManager<ProxysqlAdminUiWebUser> _userManager;

    public DefaultUserSeedService(ProxysqlAdminUiWebAuthContext dbContext, 
        IConfiguration configuration, 
        ILogger<DefaultUserSeedService> logger,
        UserManager<ProxysqlAdminUiWebUser> userManager)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task SeedDefaultUsersAsync()
    {
        try
        {
            await _dbContext.Database.EnsureCreatedAsync();
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await _dbContext.Database.MigrateAsync();
            }

            var existingUsers = await _dbContext.Users.AnyAsync();
            
            if (!existingUsers)
            {
                var defaultUsers = _configuration.GetSection("DefaultUsers")
                    .Get<List<DefaultUserModel>>() ?? new List<DefaultUserModel>();

                foreach (var user in defaultUsers)
                {

                    var result = await _userManager.CreateAsync(new ProxysqlAdminUiWebUser()
                    {
                        UserName = user.Username,
                        EmailConfirmed = true,
                        Email = user.Username+"@localhost"
                    }, user.Password);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogCritical($"{error.Code}: {error.Description}");
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    var newUser = await _userManager.Users.FirstAsync(x => x.UserName == user.Username);
                     
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                     
                    await _userManager.ConfirmEmailAsync(newUser, token);
                    
                    _logger.LogInformation($"Added default user: {user.Username}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default users");
        }
    }
}