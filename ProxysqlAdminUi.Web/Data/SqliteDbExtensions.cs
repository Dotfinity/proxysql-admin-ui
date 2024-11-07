using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.Web.Data;

public static class SqliteDbExtensions
{
    public static void SetupIdentityDbContext(this WebApplicationBuilder builder)
    {
        // TODO: set the directory from the configuration and/or environment variables
        // for docker, it would be /app/db and/or outside of the app directory, so it's not lost when the container is removed

        var currentDirectory = Directory.GetCurrentDirectory();

        var dbFolder = Path.Combine(currentDirectory, "db");

        if (!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }

        var dbPath = Path.Combine(dbFolder, "app.db");

        var connectionString = $"Data Source={dbPath}";

        // Add services to the container.
        builder.Services.AddDbContext<ProxysqlAdminUiWebAuthContext>(options => options.UseSqlite(connectionString,
            b =>
            {
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            }));
    }
}
