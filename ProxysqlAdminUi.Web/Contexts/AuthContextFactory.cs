using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Data;

namespace ProxysqlAdminUi.Web.Contexts;

/// <summary>
/// This is used primarily for migrations via the CLI
/// </summary>
public class AuthContextFactory : IDesignTimeDbContextFactory<ProxysqlAdminUiWebAuthContext>
{
    public ProxysqlAdminUiWebAuthContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProxysqlAdminUiWebAuthContext>();
        optionsBuilder.UseSqlite($"Data Source={SqliteDbExtensions.GetAppDbPath()};");

        return new ProxysqlAdminUiWebAuthContext(optionsBuilder.Options);
    }
}
