namespace ProxysqlAdminUi.Web.Extensions;

/// <summary>
/// Derives query-cache figures from the cumulative <c>stats_mysql_global</c> counters.
/// Every counter is a running total since ProxySQL started, so anything derived from them
/// is an all-time average rather than a current rate.
/// </summary>
public static class CacheStatsHelper
{
    /// <summary>
    /// The query cache hit rate as a percentage (0-100) of <c>Query_Cache_count_GET_OK</c> over
    /// <c>Query_Cache_count_GET</c>, or <c>null</c> when there has been no cache activity at all
    /// (<paramref name="get"/> is zero) and a ratio would be misleading rather than 0 percent.
    /// </summary>
    /// <param name="get">Total cache lookups (<c>Query_Cache_count_GET</c>).</param>
    /// <param name="getOk">Lookups served from the cache (<c>Query_Cache_count_GET_OK</c>).</param>
    public static double? HitRate(long get, long getOk)
    {
        if (get <= 0)
        {
            return null;
        }

        // The two counters come from the same snapshot in practice, but clamp anyway: the gauge
        // that displays this has a fixed 0-100 scale, and a negative or >100 % rate is never
        // a meaningful answer.
        var rate = Math.Round((double)getOk / get * 100, 1);
        return Math.Clamp(rate, 0, 100);
    }

    /// <summary>
    /// Lookups that had to go to a backend: <paramref name="get"/> minus <paramref name="getOk"/>,
    /// never negative even if the two counters are read a moment apart.
    /// </summary>
    public static long Misses(long get, long getOk) => Math.Max(0, get - getOk);
}
