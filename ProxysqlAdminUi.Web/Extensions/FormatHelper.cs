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
    
    
    public static string FormatMicroseconds(long microseconds)
    {
        var timeSpan = TimeSpan.FromTicks(microseconds * 10); // Convert to ticks (1 microsecond = 10 ticks)
        
        return timeSpan.TotalHours >= 1 
            ? $"{timeSpan:hh\\:mm\\:ss\\.ffffff}"
            : timeSpan.TotalMinutes >= 1 
                ? $"{timeSpan:mm\\:ss\\.ffffff}" 
                : $"{timeSpan:ss\\.ffffff}";
    }
}
