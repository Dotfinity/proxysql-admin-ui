namespace ProxysqlAdminUi.Web.Extensions;

public static class FormatHelper
{
    public static string FormatUptime(long seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return $"{(int)time.TotalDays}d {time.Hours}h {time.Minutes}m";
    }

    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double traffic = bytes;

        while (traffic >= 1024 && order < sizes.Length - 1)
        {
            order++;
            traffic /= 1024;
        }

        return $"{traffic:0.##} {sizes[order]}";
    }

    public static string FormatLargeNumber(long number)
    {
        if (number >= 1_000_000_000)
            return $"{number / 1_000_000_000.0:0.##}B";
        if (number >= 1_000_000)
            return $"{number / 1_000_000.0:0.##}M";
        if (number >= 1_000)
            return $"{number / 1_000.0:0.##}K";

        return number.ToString();
    }


    public static string FormatMiliseconds(long miliseconds)
    {
        var ts = TimeSpan.FromMilliseconds(miliseconds);

        return ts.Days > 0 ? $"{ts.Days}d {ts:hh\\:mm\\:ss\\.fff}" :
            ts.Hours > 0 ? $"{ts:hh\\:mm\\:ss\\.fff}" :
            ts.Minutes > 0 ? $"{ts:mm\\:ss\\.fff}" :
            $"{ts:ss\\.fff}";
    }


    /// <summary>
    /// Shortens a query digest for display in a grid cell. ProxySQL reports a NULL digest text
    /// for some internal statements, so a missing text yields an empty string rather than throwing.
    /// </summary>
    /// <param name="text">The digest text as returned by ProxySQL, possibly null.</param>
    /// <param name="maxLength">Characters to keep before the ellipsis.</param>
    public static string TruncateDigestText(string? text, int maxLength = 30)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "...") : text;
    }


    public static string FormatMicroseconds(long microseconds)
    {
        var ts = TimeSpan.FromMicroseconds(microseconds);
        
        return ts.Days > 0 ? $"{ts.Days}d {ts:hh\\:mm\\:ss\\.ffffff}" :
            ts.Hours > 0 ? $"{ts:hh\\:mm\\:ss\\.ffffff}" :
            ts.Minutes > 0 ? $"{ts:mm\\:ss\\.ffffff}" : 
            $"{ts:ss\\.ffffff}";
    }
}
