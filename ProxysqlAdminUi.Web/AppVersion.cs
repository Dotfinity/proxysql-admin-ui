using System.Reflection;

namespace ProxysqlAdminUi.Web;

/// <summary>
/// The version string shown in the UI. CI builds set <c>BUILD_VERSION</c>/<c>BUILD_SUFFIX</c> and
/// are the only builds that correspond to a published GitHub release; every other build falls back
/// to the assembly's informational version so the footer never advertises a release tag that does
/// not exist.
/// </summary>
public class AppVersion
{
    public AppVersion()
    {
        var buildVersion = Environment.GetEnvironmentVariable("BUILD_VERSION");
        var buildSuffix = Environment.GetEnvironmentVariable("BUILD_SUFFIX");

        // A release tag is "{date}-{run id}"; both halves come from CI. A version without a suffix
        // cannot match a tag, so it is displayed as-is but not treated as a release.
        if (!string.IsNullOrWhiteSpace(buildVersion))
        {
            var hasSuffix = !string.IsNullOrWhiteSpace(buildSuffix);
            Version = hasSuffix ? $"{buildVersion.Trim()}-{buildSuffix!.Trim()}" : buildVersion.Trim();
            IsRelease = hasSuffix;
            return;
        }

        Version = GetInformationalVersion() ?? "dev";
        IsRelease = false;
    }

    /// <summary>
    /// The displayed version, e.g. <c>2025.11.04-123456</c> for a CI build or <c>1.0.0</c>/<c>dev</c> otherwise.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// True only for builds stamped by CI, i.e. the ones whose <see cref="Version"/> matches a
    /// GitHub release tag and may therefore be rendered as a link to that release.
    /// </summary>
    public bool IsRelease { get; }

    private static string? GetInformationalVersion()
    {
        var informationalVersion = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return null;
        }

        // Strip the source-revision suffix the SDK appends, e.g. "1.0.0+9f3c1ab".
        var suffixIndex = informationalVersion.IndexOf('+');
        var version = suffixIndex >= 0 ? informationalVersion[..suffixIndex] : informationalVersion;

        return string.IsNullOrWhiteSpace(version) ? null : version;
    }
}
