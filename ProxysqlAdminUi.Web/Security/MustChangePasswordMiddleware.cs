namespace ProxysqlAdminUi.Web.Security;

/// <summary>
/// Confines a signed-in user who still carries <see cref="PaiClaims.MustChangePassword"/> to the
/// change-password page, so the published default credentials cannot be left in place.
/// </summary>
/// <remarks>
/// Must be registered after <c>UseAuthentication</c>/<c>UseAuthorization</c> - it reads
/// <see cref="HttpContext.User"/>, which the authentication middleware populates from the cookie.
/// No database round-trip is involved: the claim travels in the cookie.
/// </remarks>
public sealed class MustChangePasswordMiddleware(RequestDelegate next)
{
    /// <summary>Where a user carrying the claim is sent.</summary>
    public const string ChangePasswordPath = "/Account/Manage/ChangePassword";

    /// <summary>
    /// Paths that stay reachable while the claim is set, so the redirect cannot trap the user or
    /// loop on itself. Blazor's framework/static endpoints are exempt because redirecting them
    /// breaks the very page the user is being sent to.
    /// </summary>
    private static readonly string[] AllowedPathPrefixes =
    [
        ChangePasswordPath,
        "/Account/Logout",
        "/Account/Login",
        "/Account/Lockout",
        "/Account/AccessDenied",
        "/_framework",
        "/_content",
        "/_blazor"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (ShouldRedirect(context))
        {
            context.Response.Redirect(ChangePasswordPath);
            return;
        }

        await next(context);
    }

    private static bool ShouldRedirect(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true
            || !context.User.HasClaim(PaiClaims.MustChangePassword, PaiClaims.TrueValue))
        {
            return false;
        }

        return !IsExempt(context.Request.Path);
    }

    private static bool IsExempt(PathString path)
    {
        var value = path.Value;

        if (value is null)
        {
            return false;
        }

        foreach (var prefix in AllowedPathPrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Static assets (.css, .js, .png, ...) are served by their own middleware but may still
        // reach here; a redirect would replace them with HTML and break the page being rendered.
        return Path.HasExtension(value);
    }
}

/// <summary>
/// Registration helper for <see cref="MustChangePasswordMiddleware"/>.
/// </summary>
public static class MustChangePasswordMiddlewareExtensions
{
    /// <summary>
    /// Adds the forced-password-change redirect. Call after <c>UseAuthentication</c> and
    /// <c>UseAuthorization</c>.
    /// </summary>
    public static IApplicationBuilder UseMustChangePassword(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<MustChangePasswordMiddleware>();
    }
}
