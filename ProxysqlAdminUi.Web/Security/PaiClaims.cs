namespace ProxysqlAdminUi.Web.Security;

/// <summary>
/// Claim types this application adds on top of the ASP.NET Core Identity defaults.
/// </summary>
/// <remarks>
/// These are stored as Identity user claims (<c>AspNetUserClaims</c>) rather than as columns on
/// <see cref="Data.ProxysqlAdminUiWebUser"/>, so flags can be added without an Identity schema
/// change - the project creates the SQLite schema with <c>EnsureCreatedAsync</c> and keeps its
/// migrations empty, which makes new columns awkward to roll out to existing deployments.
/// </remarks>
public static class PaiClaims
{
    /// <summary>
    /// Present (with the value <see cref="TrueValue"/>) while the user still signs in with the
    /// documented default password. The default <c>UserClaimsPrincipalFactory</c> copies user
    /// claims into the authentication cookie, so
    /// <see cref="MustChangePasswordMiddleware"/> can check it without hitting the database.
    /// </summary>
    public const string MustChangePassword = "pai:must_change_password";

    /// <summary>The only value <see cref="MustChangePassword"/> is ever issued with.</summary>
    public const string TrueValue = "true";
}
