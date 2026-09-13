namespace ProxysqlAdminUi.Web.Components.CustomComponents;

/// <summary>
/// Converts between a duration in milliseconds and its hours/minutes/seconds/milliseconds parts.
/// A total of zero maps to <c>null</c> rather than <c>0</c>: ProxySQL treats <c>cache_ttl = 0</c>
/// differently from <c>NULL</c> (the rule does not cache at all), and the model annotates the
/// value with <c>[Range(1, int.MaxValue)]</c>.
/// </summary>
public static class TimeInputConverter
{
    private const long MillisecondsPerHour = 3_600_000L;
    private const long MillisecondsPerMinute = 60_000L;
    private const long MillisecondsPerSecond = 1_000L;

    /// <summary>
    /// Combines the four parts into a total in milliseconds, or <c>null</c> when the total is zero.
    /// Negative parts count as zero and the total saturates at <see cref="int.MaxValue"/>.
    /// </summary>
    public static int? ToMilliseconds(int hours, int minutes, int seconds, int milliseconds)
    {
        var total = (Math.Max(hours, 0) * MillisecondsPerHour)
                    + (Math.Max(minutes, 0) * MillisecondsPerMinute)
                    + (Math.Max(seconds, 0) * MillisecondsPerSecond)
                    + Math.Max(milliseconds, 0);

        if (total == 0)
        {
            return null;
        }

        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    /// <summary>
    /// Splits a total in milliseconds into its parts. <c>null</c> and negative totals yield all zeros.
    /// </summary>
    public static (int Hours, int Minutes, int Seconds, int Milliseconds) Decompose(int? totalMilliseconds)
    {
        var total = Math.Max(totalMilliseconds.GetValueOrDefault(), 0);

        return (
            (int)(total / MillisecondsPerHour),
            (int)(total % MillisecondsPerHour / MillisecondsPerMinute),
            (int)(total % MillisecondsPerMinute / MillisecondsPerSecond),
            (int)(total % MillisecondsPerSecond));
    }
}
