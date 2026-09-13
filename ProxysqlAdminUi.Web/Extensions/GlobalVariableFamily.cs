namespace ProxysqlAdminUi.Web.Extensions;

/// <summary>
/// Maps a ProxySQL global variable to the configuration family whose LOAD/SAVE statements
/// make an edit take effect and survive a restart. ProxySQL keeps every variable in a single
/// <c>global_variables</c> table, but each prefix belongs to a different family and applying
/// the wrong family silently leaves the change inert.
/// </summary>
public static class GlobalVariableFamily
{
    /// <summary>Family of the <c>mysql-*</c> variables, as spelled in "LOAD ... TO RUNTIME".</summary>
    public const string MySql = "MYSQL VARIABLES";

    /// <summary>Family of the <c>admin-*</c> variables, as spelled in "LOAD ... TO RUNTIME".</summary>
    public const string Admin = "ADMIN VARIABLES";

    /// <summary>
    /// The LOAD/SAVE family for <paramref name="variableName"/>, or <c>null</c> when the prefix
    /// belongs to a family this UI cannot apply (<c>sqliteserver-</c>, <c>clickhouse-</c>,
    /// <c>ldap-</c> or anything unknown). The match is anchored and case-insensitive.
    /// </summary>
    public static string? Resolve(string? variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return null;
        }

        if (variableName.StartsWith("mysql-", StringComparison.OrdinalIgnoreCase))
        {
            return MySql;
        }

        if (variableName.StartsWith("admin-", StringComparison.OrdinalIgnoreCase))
        {
            return Admin;
        }

        return null;
    }

    /// <summary>Whether an edit to <paramref name="variableName"/> can be applied by this UI.</summary>
    public static bool IsEditable(string? variableName) => Resolve(variableName) is not null;

    /// <summary>
    /// The prefix up to and including the first '-', used in the message that explains why a
    /// variable is read-only. Returns the whole name when it carries no prefix.
    /// </summary>
    public static string PrefixOf(string? variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return string.Empty;
        }

        var separator = variableName.IndexOf('-', StringComparison.Ordinal);
        return separator < 0 ? variableName : variableName[..(separator + 1)];
    }

    /// <summary>Explains why a variable of an unsupported family cannot be saved.</summary>
    public static string NotEditableMessage(string? variableName) =>
        $"Variables with prefix '{PrefixOf(variableName)}' cannot be edited from this UI.";
}
