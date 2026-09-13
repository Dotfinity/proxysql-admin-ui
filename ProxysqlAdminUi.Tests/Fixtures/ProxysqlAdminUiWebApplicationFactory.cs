using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ProxysqlAdminUi.Tests.Fixtures;

/// <summary>
/// Boots the web application in-process on <see cref="Microsoft.AspNetCore.TestHost.TestServer"/>.
/// </summary>
/// <remarks>
/// The application resolves its SQLite identity database through
/// <c>SqliteDbExtensions.GetAppDbPath()</c>, which reads the <c>APP_DB_PATH</c> environment
/// variable directly rather than from <see cref="IConfiguration"/>. The variable therefore has to
/// be set on the process before the host is built, which is why it happens in the constructor.
/// </remarks>
public sealed class ProxysqlAdminUiWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string AppDbPathVariable = "APP_DB_PATH";

    private const string ProxySqlConnectionStringKey = "ConnectionStrings:ProxySqlContext";

    /// <summary>
    /// Port 1 is not listening, so any attempt to open a ProxySQL connection from a test fails
    /// immediately rather than reaching a real server or blocking on a TCP connect.
    /// </summary>
    public const string UnreachableProxySqlConnectionString =
        "Server=127.0.0.1;Port=1;Uid=x;Pwd=x;ConnectionTimeout=1;";

    private readonly string _appDbRoot;
    private readonly string? _originalAppDbPath;

    public ProxysqlAdminUiWebApplicationFactory()
    {
        _originalAppDbPath = Environment.GetEnvironmentVariable(AppDbPathVariable);

        _appDbRoot = Path.Combine(
            Path.GetTempPath(),
            "proxysql-admin-ui-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_appDbRoot);
        Environment.SetEnvironmentVariable(AppDbPathVariable, _appDbRoot);
    }

    /// <summary>The isolated directory that holds this factory's <c>db/app.db</c>.</summary>
    public string AppDbRoot => _appDbRoot;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Keeps the developer exception page on so a startup failure surfaces in the test output
        // instead of being swallowed by the "/Error" handler.
        builder.UseEnvironment(Environments.Development);

        // Development is also the environment whose appsettings.Development.json carries a
        // developer's real ProxySQL endpoint and credentials. Point the MySQL context at a closed
        // local port with a one-second timeout so a test can never reach a live ProxySQL instance,
        // and so a page that does touch the repository fails fast instead of hanging on a connect.
        builder.UseSetting(ProxySqlConnectionStringKey, UnreachableProxySqlConnectionString);

        // UseSetting on its own is NOT enough here, and silently so. Program.cs re-runs
        // AddJsonFile("appsettings.json") / AddJsonFile($"appsettings.{Environment}.json") on the
        // builder's ConfigurationManager, and those sources are appended after the host settings,
        // so the developer's appsettings.Development.json wins the last-source-wins lookup.
        // ConfigureAppConfiguration runs later still, so appending an in-memory source here is what
        // actually takes effect. WebApplicationSmokeTests asserts the resulting value so this cannot
        // regress back into "the test suite quietly talks to a real server".
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ProxySqlConnectionStringKey] = UnreachableProxySqlConnectionString
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        Environment.SetEnvironmentVariable(AppDbPathVariable, _originalAppDbPath);

        // SQLite may still hold the file handle briefly after the host is torn down; a failed
        // cleanup of a temp directory must never fail the test run.
        try
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            if (Directory.Exists(_appDbRoot))
            {
                Directory.Delete(_appDbRoot, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
