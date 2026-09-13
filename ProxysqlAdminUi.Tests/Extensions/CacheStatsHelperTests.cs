using AwesomeAssertions;
using ProxysqlAdminUi.Web.Extensions;

namespace ProxysqlAdminUi.Tests.Extensions;

/// <summary>
/// Characterisation tests for <see cref="CacheStatsHelper"/> - both helpers read the
/// cumulative <c>stats_mysql_global</c> counters, so the interesting cases are the ones the
/// dashboard hits when ProxySQL has just started or when the two counters are sampled a moment
/// apart. Every expectation below is what the implementation produces today - a failure means a
/// dashboard number changed, so decide deliberately before updating.
/// </summary>
public class CacheStatsHelperTests
{
    public class HitRate
    {
        [Theory]
        // "No cache activity yet" is reported as null rather than 0 percent, so the UI can show a
        // placeholder instead of a misleading 0 percent hit rate.
        [InlineData(0L, 0L)]
        [InlineData(0L, 5L)]
        // Counters are unsigned in ProxySQL, but the guard is <= 0 rather than == 0, so a negative
        // value read out of a signed column is treated as "no activity" too.
        [InlineData(-1L, 0L)]
        [InlineData(-100L, 50L)]
        [InlineData(long.MinValue, 0L)]
        public void WithNonPositiveGet_ReturnsNull(long get, long getOk)
        {
            CacheStatsHelper.HitRate(get, getOk).Should().BeNull(
                "a ratio over zero lookups is meaningless and the caller is expected to render a placeholder");
        }

        [Theory]
        // The result is a percentage (0-100), not a 0-1 fraction.
        [InlineData(100L, 66L, 66.0)]
        [InlineData(100L, 0L, 0.0)]
        [InlineData(100L, 100L, 100.0)]
        [InlineData(1L, 1L, 100.0)]
        [InlineData(2L, 1L, 50.0)]
        [InlineData(long.MaxValue, long.MaxValue, 100.0)]
        public void WithWholePercentages_ReturnsThePercentage(long get, long getOk, double expected)
        {
            CacheStatsHelper.HitRate(get, getOk).Should().Be(expected);
        }

        [Theory]
        // Rounding is to ONE decimal place, not two: 1/3 is 33.3, never 33.33.
        [InlineData(3L, 1L, 33.3)]
        [InlineData(3L, 2L, 66.7)]
        [InlineData(7L, 1L, 14.3)]
        [InlineData(1_000L, 1L, 0.1)]
        // Below 0.05 percent the rounding collapses a non-zero hit rate to a flat 0.
        [InlineData(10_000L, 1L, 0.0)]
        public void WithFractionalPercentages_RoundsToOneDecimal(long get, long getOk, double expected)
        {
            CacheStatsHelper.HitRate(get, getOk).Should().Be(expected);
        }

        [Fact]
        public void AtAnExactMidpoint_RoundsToEven()
        {
            // 1/16 is exactly 6.25 percent in binary, so this really is a midpoint. Math.Round's
            // default is MidpointRounding.ToEven, so it goes down to 6.2 rather than up to 6.3.
            CacheStatsHelper.HitRate(16, 1).Should().Be(
                6.2,
                "Math.Round defaults to banker's rounding - a switch to AwayFromZero would move this to 6.3");
        }

        [Theory]
        // The gauge that renders this has a fixed 0-100 scale, so skewed counters (GET_OK read
        // a moment after GET on a busy server) must never push the value above 100.
        [InlineData(100L, 150L, 100.0)]
        [InlineData(100L, 101L, 100.0)]
        [InlineData(1L, 1_000L, 100.0)]
        public void WithGetOkAboveGet_IsClampedToOneHundredPercent(long get, long getOk, double expected)
        {
            CacheStatsHelper.HitRate(get, getOk).Should().Be(expected);
        }

        [Theory]
        // A negative getOk is impossible in practice; it is clamped to 0 rather than rendered.
        [InlineData(100L, -50L, 0.0)]
        public void WithNegativeGetOk_IsClampedToZero(long get, long getOk, double expected)
        {
            CacheStatsHelper.HitRate(get, getOk).Should().Be(expected);
        }
    }

    public class Misses
    {
        [Theory]
        [InlineData(100L, 66L, 34L)]
        [InlineData(0L, 0L, 0L)]
        [InlineData(1L, 0L, 1L)]
        [InlineData(100L, 100L, 0L)]
        [InlineData(long.MaxValue, 0L, long.MaxValue)]
        public void WithConsistentCounters_ReturnsTheDifference(long get, long getOk, long expected)
        {
            CacheStatsHelper.Misses(get, getOk).Should().Be(expected);
        }

        [Theory]
        // Unlike HitRate, Misses IS clamped: a skewed sample floors at zero instead of going negative.
        [InlineData(100L, 120L, 0L)]
        [InlineData(0L, 1L, 0L)]
        [InlineData(-5L, 0L, 0L)]
        [InlineData(-100L, 50L, 0L)]
        public void WithGetOkAboveGet_ClampsToZeroRatherThanGoingNegative(long get, long getOk, long expected)
        {
            CacheStatsHelper.Misses(get, getOk).Should().Be(
                expected,
                "the counters are sampled separately, so getOk can legitimately overtake get");
        }

        [Fact]
        public void WithCountersThatOverflowTheSubtraction_WrapsAroundAndLosesTheClamp()
        {
            // PINNED: get - getOk is unchecked long arithmetic. long.MaxValue - long.MinValue
            // overflows to -1, which Math.Max then floors to 0 - so the clamp accidentally saves
            // this one, but the intermediate value is garbage. The reverse case is the real
            // hazard: long.MinValue - long.MaxValue wraps to 1 and is reported as a real miss.
            CacheStatsHelper.Misses(long.MaxValue, long.MinValue).Should().Be(0);
            CacheStatsHelper.Misses(long.MinValue, long.MaxValue).Should().Be(1);
        }
    }
}
