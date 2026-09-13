using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProxysqlAdminUi.Web.Contexts;

namespace ProxysqlAdminUi.Web.Data;

public static class SqliteDbExtensions
{
    public static void SetupIdentityDbContext(this WebApplicationBuilder builder)
    {
        var dbPath = GetAppDbPath();

        var connectionString = $"Data Source={dbPath}";

        // Add services to the container.
        builder.Services.AddDbContext<ProxysqlAdminUiWebAuthContext>(options => options
            .UseSqlite(connectionString, b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
            // The schema is created from the model by EnsureCreatedAsync (see DefaultUserSeedService) and the
            // migrations only track the snapshot, so a snapshot/model mismatch must not abort MigrateAsync
            // (EF Core 9+ throws on this warning by default, which silently skipped default user seeding).
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
    }

    public static string GetAppDbPath()
    {
        var currentDirectory = "/app/";

        var envVarPath = Environment.GetEnvironmentVariable("APP_DB_PATH");
        
        if (!string.IsNullOrEmpty(envVarPath))
        {
            currentDirectory = envVarPath;
        }
        else
        { 
            currentDirectory = Directory.GetCurrentDirectory();
        }


        var dbFolder = Path.Combine(currentDirectory, "db");

        if (!Directory.Exists(dbFolder))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Directory.CreateDirectory(dbFolder);
            }
            else
            {
                Directory.CreateDirectory(dbFolder,
                    UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        var dbPath = Path.Combine(dbFolder, "app.db");
        return dbPath;
    }
}
