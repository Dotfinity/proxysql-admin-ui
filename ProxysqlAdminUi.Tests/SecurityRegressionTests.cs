using System.Text.RegularExpressions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using ProxysqlAdminUi.Tests.Fixtures;

namespace ProxysqlAdminUi.Tests;

/// <summary>
/// Pins the security fixes (closed registration, logout, forced password change, lockout, non-routable dialogs) at the HTTP level, so a later refactor cannot quietly
/// reopen any of the holes.
/// </summary>
/// <remarks>
/// <para>
/// None of these routes read ProxySQL data, so no live ProxySQL instance is needed - the fixture
/// points the MySQL context at a closed port.
/// </para>
/// <para>
/// Two of the tests mutate the identity database (the forced password change rewrites the admin's
/// password hash, and the lockout test locks the account for five minutes). Both therefore build
/// their own <see cref="ProxysqlAdminUiWebApplicationFactory"/>, because the factory scopes the
/// SQLite file to a per-instance temp directory: sharing this class's fixture would leave every
/// later sign-in in the class - and, through APP_DB_PATH, potentially another class - facing
/// either the wrong password or a locked account. The assembly runs test classes sequentially
/// (see AssemblyInfo.cs), so a nested factory only has to restore APP_DB_PATH on dispose, which it
/// does.
/// </para>
/// </remarks>
public sealed class SecurityRegressionTests(ProxysqlAdminUiWebApplicationFactory factory)
    : IClassFixture<ProxysqlAdminUiWebApplicationFactory>
{
    #region Self-service account routes are gone

    /// <summary>
    /// Registration, the e-mail based password-reset flow and the external-login pages were
    /// deleted outright. A deleted <c>@page</c> leaves no route, so the Blazor router never matches
    /// and the request ends as a 404 - what must never come back is a 200 carrying a usable form.
    /// </summary>
    [Theory]
    [InlineData("/Account/Register")]
    [InlineData("/Account/RegisterConfirmation")]
    [InlineData("/Account/ForgotPassword")]
    [InlineData("/Account/ResetPassword")]
    [InlineData("/Account/ResendEmailConfirmation")]
    [InlineData("/Account/ConfirmEmail")]
    [InlineData("/Account/ExternalLogin")]
    [InlineData("/Account/Manage/Email")]
    [InlineData("/Account/Manage/ExternalLogins")]
    public async Task RemovedAccountRoute_WhenAnonymous_ReturnsNotFound(string path)
    {
        var client = CreateClient(factory);

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound,
            "\"{0}\" was removed; a 200 here would mean the page is back, and a redirect would "
            + "mean it merely moved behind authentication",
            path);
    }

    #endregion

    #region Sign-out is a statically rendered POST form

    /// <summary>
    /// Sign-out moved from a layout-hosted form (which had no antiforgery token, because the app
    /// renders interactively without prerendering) to the statically rendered
    /// <c>/Account/Logout</c> page. The page must therefore serve a POST form carrying both the
    /// antiforgery token and the <c>logout</c> handler name.
    /// </summary>
    [Fact]
    public async Task LogoutPage_WhenAuthenticated_RendersAntiforgeryProtectedPostForm()
    {
        var client = CreateClient(factory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var response = await client.GetAsync(LogoutPath, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        html.Should().Contain("method=\"post\"", "signing out must not be reachable by a GET");
        html.Should().Contain(
            "value=\"logout\"",
            "the form posts back to /Account/Logout and is dispatched by its FormName");
        html.Should().Contain(
            "name=\"__RequestVerificationToken\"",
            "the whole point of moving sign-out onto an SSR page was to get a real antiforgery token");
    }

    /// <summary>
    /// The POST must actually end the session. The redirect alone proves nothing - what matters
    /// is that the response expires the identity cookie.
    /// </summary>
    [Fact]
    public async Task LogoutPost_WhenAuthenticated_ExpiresTheIdentityCookieAndRedirectsToLogin()
    {
        var client = CreateClient(factory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var page = await client.GetAsync(LogoutPath, TestContext.Current.CancellationToken);
        var token = await ExtractAntiforgeryTokenAsync(page);

        var response = await client.PostAsync(
            LogoutPath,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["_handler"] = "logout",
                ["__RequestVerificationToken"] = token
            }),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        ResolvePath(client, response).Should().Be(LoginPath);

        var identityCookie = ReadSetCookie(response, IdentityApplicationCookieName);

        identityCookie.Should().NotBeNull("signing out has to clear the authentication cookie");
        var isExpired = string.IsNullOrEmpty(identityCookie!.Value.Value)
                        || identityCookie.Expires <= DateTimeOffset.UtcNow;
        isExpired.Should().BeTrue(
            "the sign-out response must expire \"{0}\" rather than leave a usable session behind",
            IdentityApplicationCookieName);
    }

    /// <summary>
    /// The page carries no <c>[Authorize]</c> attribute, so an anonymous hit - a bookmark, or a
    /// second logout in another tab - must render normally instead of throwing.
    /// </summary>
    [Fact]
    public async Task LogoutPage_WhenAnonymous_DoesNotFail()
    {
        var client = CreateClient(factory);

        var response = await client.GetAsync(LogoutPath, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "/Account/Logout is not behind [Authorize]; an anonymous visit renders the confirmation "
            + "page instead of producing a server error");
    }

    #endregion

    #region The default password is a dead end until it is changed

    /// <summary>
    /// The seeded admin still authenticates with the password published in appsettings.json, so
    /// startup flags it with <c>PaiClaims.MustChangePassword</c> and the middleware confines it to
    /// the change-password page.
    /// </summary>
    [Fact]
    public async Task Root_WhenSignedInWithTheDefaultPassword_RedirectsToChangePassword()
    {
        var client = CreateClient(factory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        ResolvePath(client, response).Should().Be(
            ChangePasswordPath,
            "a user still on the documented default password must not reach the dashboard");
    }

    /// <summary>
    /// The destination of the redirect must itself be exempt, otherwise the middleware loops.
    /// </summary>
    [Fact]
    public async Task ChangePasswordPage_WhenSignedInWithTheDefaultPassword_IsReachable()
    {
        var client = CreateClient(factory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var response = await client.GetAsync(ChangePasswordPath, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "the page the user is forced onto cannot itself be redirected away");
    }

    /// <summary>
    /// Static assets are exempt by file extension. Redirecting them would hand the browser HTML
    /// where it asked for an image or a stylesheet, and the change-password page would render broken.
    /// </summary>
    [Theory]
    [InlineData("/favicon.png", HttpStatusCode.OK)]
    // A file that is not there must still come back as a 404 rather than as HTML: the exemption is
    // by extension, not by whether the asset happens to exist.
    [InlineData("/css/not-a-real-stylesheet.css", HttpStatusCode.NotFound)]
    public async Task StaticAsset_WhenSignedInWithTheDefaultPassword_IsNotRedirected(
        string path,
        HttpStatusCode expected)
    {
        var client = CreateClient(factory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            expected,
            "a path with a file extension is exempt from the forced-password-change redirect - "
            + "serving HTML where an asset was asked for breaks the very page the user is sent to");
    }

    /// <summary>
    /// The other half: once the password is changed the claim is dropped and the dashboard opens
    /// up again. Uses its own host because it rewrites the admin's password hash.
    /// </summary>
    [Fact]
    public async Task ChangingTheDefaultPassword_ClearsTheForcedRedirect()
    {
        using var isolatedFactory = new ProxysqlAdminUiWebApplicationFactory();
        var client = CreateClient(isolatedFactory);
        await SignInAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        var page = await client.GetAsync(ChangePasswordPath, TestContext.Current.CancellationToken);
        var token = await ExtractAntiforgeryTokenAsync(page);

        var response = await client.PostAsync(
            ChangePasswordPath,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.OldPassword"] = DefaultAdminPassword,
                ["Input.NewPassword"] = ReplacementAdminPassword,
                ["Input.ConfirmPassword"] = ReplacementAdminPassword,
                ["_handler"] = "change-password",
                ["__RequestVerificationToken"] = token
            }),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.Found,
            "a rejected change re-renders the form with 200 and an error message");
        ResolvePath(client, response).Should().Be("/");

        var dashboard = await client.GetAsync("/", TestContext.Current.CancellationToken);

        // The dashboard renders interactively without prerendering, so this is only the host page -
        // enough to prove the middleware stopped intercepting.
        dashboard.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "clearing the claim must let the user through; another 302 would mean the flag survived "
            + "the password change and the user is trapped");
    }

    #endregion

    #region Lockout

    /// <summary>
    /// Five failed attempts lock the account for five minutes. Identity locks on the attempt
    /// that reaches the limit, so the fifth POST already redirects to /Account/Lockout, and every
    /// following attempt - correct password included - is refused the same way.
    /// </summary>
    /// <remarks>
    /// Runs on its own host: the lock lasts five minutes of wall-clock time and would break every
    /// later sign-in sharing the same identity database.
    /// </remarks>
    [Fact]
    public async Task Login_WithRepeatedWrongPasswords_LocksTheAccountOnTheFifthAttempt()
    {
        using var isolatedFactory = new ProxysqlAdminUiWebApplicationFactory();
        var client = CreateClient(isolatedFactory);

        const int maxFailedAccessAttempts = 5;

        for (var attempt = 1; attempt < maxFailedAccessAttempts; attempt++)
        {
            var rejected = await PostLoginAsync(client, DefaultAdminUserName, "WrongPassword!" + attempt);

            rejected.StatusCode.Should().Be(
                HttpStatusCode.OK,
                "attempt {0} is below the limit, so the login page re-renders with an "
                + "invalid-credentials message",
                attempt);
        }

        var lockingAttempt = await PostLoginAsync(client, DefaultAdminUserName, "WrongPasswordFinal!");

        lockingAttempt.StatusCode.Should().Be(HttpStatusCode.Found);
        ResolvePath(client, lockingAttempt).Should().Be(
            LockoutPath,
            "the attempt that reaches MaxFailedAccessAttempts must report the lockout, not another "
            + "invalid-credentials page");

        var correctPassword = await PostLoginAsync(client, DefaultAdminUserName, DefaultAdminPassword);

        correctPassword.StatusCode.Should().Be(HttpStatusCode.Found);
        ResolvePath(client, correctPassword).Should().Be(
            LockoutPath,
            "a lockout that the right password can walk straight through is not a lockout");
    }

    #endregion

    #region Dialog components are not routable

    /// <summary>
    /// The add/edit/delete dialogs are components opened by the Radzen dialog service, not
    /// pages. They used to carry <c>@page</c> directives, which published unauthenticated routes
    /// straight onto ProxySQL-mutating UI. With the directives gone the router has nothing to match,
    /// so the response is a 404 - not the login redirect an existing-but-protected route would give.
    /// </summary>
    [Theory]
    [InlineData("/mysql/rules/add")]
    [InlineData("/mysql/rules/edit/1")]
    [InlineData("/mysql/rules/delete/1")]
    [InlineData("/mysql/queries/digest/text")]
    public async Task DialogRoute_WhenAnonymous_ReturnsNotFound(string path)
    {
        var client = CreateClient(factory);

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound,
            "\"{0}\" must not exist as a route; a redirect to the login page would mean the route is "
            + "back and merely authenticated",
            path);
    }

    #endregion

    #region Helpers

    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminPassword = "AdminAdminAdmin1";

    /// <summary>Satisfies the 12-character / 4-unique-character policy configured in Program.cs.</summary>
    private const string ReplacementAdminPassword = "Str0ngTestPassword!23";

    private const string IdentityApplicationCookieName = ".AspNetCore.Identity.Application";
    private const string LoginPath = "/Account/Login";
    private const string LogoutPath = "/Account/Logout";
    private const string LockoutPath = "/Account/Lockout";
    private const string ChangePasswordPath = "/Account/Manage/ChangePassword";

    private static readonly Regex AntiforgeryTokenPattern = new(
        """<input\b[^>]*\bname="__RequestVerificationToken"[^>]*\bvalue="(?<token>[^"]+)""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Redirects have to be inspected rather than followed (every assertion here is about the
    /// redirect itself), and the cookie container is what carries the session between requests.
    /// </summary>
    private static HttpClient CreateClient(ProxysqlAdminUiWebApplicationFactory webApplicationFactory)
        => webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    private static async Task SignInAsync(HttpClient client, string userName, string password)
    {
        var response = await PostLoginAsync(client, userName, password);

        response.StatusCode.Should().Be(
            HttpStatusCode.Found,
            "the test needs an authenticated session; a failed sign-in re-renders the form with 200");
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(
        HttpClient client,
        string userName,
        string password)
    {
        // A fresh token per attempt: the antiforgery cookie travels in the container, but the failed
        // attempts re-render the page and there is no reason to re-use a stale field value.
        var loginPage = await client.GetAsync(LoginPath, TestContext.Current.CancellationToken);
        var token = await ExtractAntiforgeryTokenAsync(loginPage);

        return await client.PostAsync(
            LoginPath,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = userName,
                ["Input.Password"] = password,
                ["Input.RememberMe"] = "false",
                // Blazor SSR dispatches the POST to the EditForm whose FormName matches "_handler".
                ["_handler"] = "login",
                ["__RequestVerificationToken"] = token
            }),
            TestContext.Current.CancellationToken);
    }

    private static async Task<string> ExtractAntiforgeryTokenAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            "the form has to render before its antiforgery token can be posted back");

        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var match = AntiforgeryTokenPattern.Match(html);

        match.Success.Should().BeTrue("the page must render an antiforgery token to post back");

        return match.Groups["token"].Value;
    }

    /// <summary>Resolves a <c>Location</c> header, which Blazor may emit relative or absolute.</summary>
    private static string ResolvePath(HttpClient client, HttpResponseMessage response)
    {
        var location = response.Headers.Location;

        location.Should().NotBeNull("a redirect must say where it is going");

        return location!.IsAbsoluteUri
            ? location.AbsolutePath
            : new Uri(client.BaseAddress!, location).AbsolutePath;
    }

    private static SetCookieHeaderValue? ReadSetCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues(HeaderNames.SetCookie, out var values))
        {
            return null;
        }

        return SetCookieHeaderValue.ParseList([.. values])
            .FirstOrDefault(cookie => string.Equals(cookie.Name.Value, name, StringComparison.Ordinal));
    }

    #endregion
}
