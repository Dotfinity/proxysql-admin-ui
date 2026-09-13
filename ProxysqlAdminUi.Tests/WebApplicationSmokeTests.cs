using System.Text.RegularExpressions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxysqlAdminUi.Tests.Fixtures;
using ProxysqlAdminUi.Web.Data;

namespace ProxysqlAdminUi.Tests;

/// <summary>
/// Verifies the application boots end-to-end and serves its unauthenticated surface.
/// Nothing here requires a live ProxySQL instance: the MySQL connection is only opened by
/// authenticated pages.
/// </summary>
public sealed class WebApplicationSmokeTests(ProxysqlAdminUiWebApplicationFactory factory)
    : IClassFixture<ProxysqlAdminUiWebApplicationFactory>
{
    [Fact]
    public async Task Root_WhenUnauthenticated_FollowsRedirectToLoginPage()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5
        });

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.AbsolutePath
            .Should().Be("/Account/Login", "an unauthenticated request to \"/\" must land on the login page");

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("Log in to ProxySql Admin UI");
        body.Should().Contain("name=\"__RequestVerificationToken\"", "the login page renders an antiforgery-protected form");
    }

    [Fact]
    public async Task Root_WhenUnauthenticated_Returns302ToLogin()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Found);

        var location = response.Headers.Location!.ToString();
        location.Should().Contain("/Account/Login");
        // The cookie authentication challenge emits "ReturnUrl"; the Blazor RedirectToLogin
        // fallback emits "returnUrl" - either is acceptable, so compare case-insensitively.
        location.Should().ContainEquivalentOf(
            "returnurl=",
            "the redirect preserves where the user was heading");
    }

    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/account/login")]
    public async Task LoginPage_ReturnsOk(string path)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }

    [Fact]
    public void Startup_CreatesIdentityDatabaseUnderIsolatedAppDbPath()
    {
        // Forces the host to boot.
        _ = factory.CreateClient();

        var expectedDbFile = Path.Combine(factory.AppDbRoot, "db", "app.db");

        File.Exists(expectedDbFile).Should()
            .BeTrue("APP_DB_PATH must redirect the identity database into the test's temp directory");
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/this-route-does-not-exist",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Regression test for the .NET 10 upgrade: <c>DefaultUserSeedService</c> wraps its whole body in
    /// a catch-all that only logs, so when <c>MigrateAsync</c> started throwing
    /// <c>PendingModelChangesWarning</c> the host still started perfectly happily with an empty
    /// identity database and nobody could log in. Asserting on the seeded user - not on the logs -
    /// is what actually catches that.
    /// </summary>
    [Fact]
    public async Task Startup_SeedsDefaultAdminUser()
    {
        // Forces the host (and therefore the seeding that runs before the pipeline is built) to run.
        _ = factory.CreateClient();

        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ProxysqlAdminUiWebUser>>();

        var admin = await userManager.FindByNameAsync(DefaultAdminUserName);

        admin.Should().NotBeNull(
            "startup must seed the \"DefaultUsers\" entry from appsettings.json - a swallowed seeding "
            + "failure leaves the application with no way in");
        admin.EmailConfirmed.Should().BeTrue(
            "the seeded account confirms its own e-mail so the first login is not blocked");
    }

    /// <summary>
    /// End-to-end companion to <see cref="Startup_SeedsDefaultAdminUser"/>: proves the seeded
    /// credentials actually authenticate through the Blazor SSR login form, password hash included.
    /// </summary>
    [Fact]
    public async Task Login_WithDefaultAdminCredentials_Succeeds()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginPage = await client.GetAsync("/Account/Login", TestContext.Current.CancellationToken);
        loginPage.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await loginPage.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var antiforgeryToken = ExtractAntiforgeryToken(html);

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = DefaultAdminUserName,
            ["Input.Password"] = DefaultAdminPassword,
            ["Input.RememberMe"] = "false",
            // Blazor SSR dispatches the POST to the EditForm whose FormName matches "_handler".
            ["_handler"] = "login",
            ["__RequestVerificationToken"] = antiforgeryToken
        });

        var response = await client.PostAsync(
            "/Account/Login",
            form,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.Found,
            "a successful sign-in redirects; a failed one re-renders the form with 200 OK");

        var location = response.Headers.Location!;
        var path = location.IsAbsoluteUri
            ? location.AbsolutePath
            : new Uri(client.BaseAddress!, location).AbsolutePath;
        path.Should().Be("/", "an empty ReturnUrl sends the signed-in user to the dashboard");

        response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
        setCookies.Should().Contain(
            cookie => cookie.StartsWith(IdentityApplicationCookieName, StringComparison.Ordinal),
            "the sign-in must issue the identity cookie, not just redirect");
    }

    /// <summary>
    /// The fixture runs the host in the Development environment, whose appsettings.Development.json
    /// holds a developer's real ProxySQL endpoint and credentials. Program.cs re-adds that JSON file
    /// to the configuration after the test host's settings are applied, so the override winning is
    /// not self-evident - assert it, rather than discover a test suite talking to a live server.
    /// </summary>
    [Fact]
    public void Configuration_ProxySqlConnectionString_IsOverriddenByTheTestHost()
    {
        _ = factory.CreateClient();

        var configuration = factory.Services.GetRequiredService<IConfiguration>();

        configuration.GetConnectionString("ProxySqlContext").Should().Be(
            ProxysqlAdminUiWebApplicationFactory.UnreachableProxySqlConnectionString,
            "a test must never inherit a developer's real ProxySQL connection string");
    }

    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminPassword = "AdminAdminAdmin1";
    private const string IdentityApplicationCookieName = ".AspNetCore.Identity.Application";

    private static readonly Regex AntiforgeryTokenPattern = new(
        """<input\b[^>]*\bname="__RequestVerificationToken"[^>]*\bvalue="(?<token>[^"]+)""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = AntiforgeryTokenPattern.Match(html);

        match.Success.Should().BeTrue("the login page must render an antiforgery token to post back");

        return match.Groups["token"].Value;
    }
}
