using System.Security.Claims;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using ProxysqlAdminUi.Web.Security;

namespace ProxysqlAdminUi.Tests.Security;

/// <summary>
/// Unit-level companion to the HTTP tests in <see cref="SecurityRegressionTests"/>: the exempt-path
/// table is the part of the forced-password-change flow most likely to be edited by someone adding a route, and it is far cheaper
/// to pin here than by booting a host per case.
/// </summary>
public sealed class MustChangePasswordMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenUserMustChangePassword_RedirectsAndDoesNotCallNext()
    {
        var context = CreateContext("/", WithMustChangePasswordClaim());
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        context.Response.Headers.Location.ToString().Should()
            .Be(MustChangePasswordMiddleware.ChangePasswordPath);
        nextCalled.Should().BeFalse("the request must not reach the page the user is being kept off");
    }

    /// <summary>
    /// Every one of these has to stay reachable while the claim is set, otherwise the redirect either
    /// loops on itself or traps the user with no way to sign out.
    /// </summary>
    [Theory]
    [InlineData("/Account/Manage/ChangePassword")]
    [InlineData("/Account/Manage/ChangePassword?returnUrl=%2F")]
    [InlineData("/Account/Logout")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Lockout")]
    [InlineData("/Account/AccessDenied")]
    [InlineData("/_framework/blazor.web.js")]
    [InlineData("/_content/Radzen.Blazor/Radzen.Blazor.js")]
    [InlineData("/_blazor")]
    // Matching is case-insensitive: Blazor's own links use a different casing from the table.
    [InlineData("/account/logout")]
    [InlineData("/ACCOUNT/MANAGE/CHANGEPASSWORD")]
    // Anything with a file extension is a static asset; replacing it with HTML breaks the page.
    [InlineData("/favicon.png")]
    [InlineData("/css/site.css")]
    public async Task InvokeAsync_WithExemptPath_CallsNext(string path)
    {
        var context = CreateContext(path, WithMustChangePasswordClaim());
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        nextCalled.Should().BeTrue("\"{0}\" is exempt from the forced-password-change redirect", path);
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/mysql/servers")]
    [InlineData("/Account/Manage")]
    [InlineData("/Account/Manage/TwoFactorAuthentication")]
    public async Task InvokeAsync_WithProtectedPath_Redirects(string path)
    {
        var context = CreateContext(path, WithMustChangePasswordClaim());
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedWithoutTheClaim_CallsNext()
    {
        var context = CreateContext("/", new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], AuthenticationType)));
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        nextCalled.Should().BeTrue("a user who has changed their password is not confined");
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    /// <summary>
    /// The claim is only meaningful on an authenticated principal - an anonymous request has to fall
    /// through to the normal authentication challenge, not be bounced to a page it cannot use.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAnonymous_CallsNext()
    {
        var context = CreateContext("/", new ClaimsPrincipal(new ClaimsIdentity()));
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    /// <summary>
    /// An unauthenticated identity that somehow carries the claim (a forged or stale cookie that was
    /// not validated) must not be treated as if it were signed in.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenClaimIsPresentButIdentityIsNotAuthenticated_CallsNext()
    {
        var context = CreateContext("/", new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(PaiClaims.MustChangePassword, PaiClaims.TrueValue)])));
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        nextCalled.Should().BeTrue("an unauthenticated identity carries no trustworthy claims");
    }

    /// <summary>
    /// The claim is matched on its value as well as its type, so an explicitly cleared flag does not
    /// keep redirecting.
    /// </summary>
    [Theory]
    [InlineData("false")]
    [InlineData("True")]
    [InlineData("")]
    public async Task InvokeAsync_WhenClaimValueIsNotTrue_CallsNext(string claimValue)
    {
        var context = CreateContext("/", new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(PaiClaims.MustChangePassword, claimValue)], AuthenticationType)));
        var nextCalled = false;

        await CreateSut(() => nextCalled = true).InvokeAsync(context);

        nextCalled.Should().BeTrue(
            "only the exact value \"{0}\" arms the redirect", PaiClaims.TrueValue);
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_Throws()
        => await Assert.ThrowsAsync<ArgumentNullException>(
            () => CreateSut(() => { }).InvokeAsync(null!));

    private const string AuthenticationType = "TestAuthentication";

    private static MustChangePasswordMiddleware CreateSut(Action onNext)
        => new(_ =>
        {
            onNext();
            return Task.CompletedTask;
        });

    private static ClaimsPrincipal WithMustChangePasswordClaim()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(PaiClaims.MustChangePassword, PaiClaims.TrueValue)
            ],
            AuthenticationType));

    private static DefaultHttpContext CreateContext(string pathAndQuery, ClaimsPrincipal user)
    {
        var context = new DefaultHttpContext { User = user };

        var queryStart = pathAndQuery.IndexOf('?', StringComparison.Ordinal);

        if (queryStart < 0)
        {
            context.Request.Path = pathAndQuery;
        }
        else
        {
            context.Request.Path = pathAndQuery[..queryStart];
            context.Request.QueryString = new QueryString(pathAndQuery[queryStart..]);
        }

        return context;
    }
}
