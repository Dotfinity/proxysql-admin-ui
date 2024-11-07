using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.Web.Data;

public class AuthContextFactory : IDesignTimeDbContextFactory<ProxysqlAdminUiWebAuthContext>
{
    public ProxysqlAdminUiWebAuthContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProxysqlAdminUiWebAuthContext>();
        //TODO: this also needs to be set from the configuration and/or environment variables
        optionsBuilder.UseSqlite("Data Source=/db/app.db;");

        return new ProxysqlAdminUiWebAuthContext(optionsBuilder.Options);
    }
}
