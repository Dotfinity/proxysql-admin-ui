using AwesomeAssertions;
using ProxysqlAdminUi.Web.Components.CustomComponents;

namespace ProxysqlAdminUi.Tests.Components;

/// <summary>
/// Covers <see cref="TimeInputConverter"/>, the pure conversion behind the TimeInput component
/// The two documented quirks - a zero total maps to <c>null</c> rather than <c>0</c>,
/// and the total saturates at <see cref="int.MaxValue"/> instead of throwing - are the whole point
/// of the type, so they are pinned explicitly here.
/// </summary>
public class TimeInputConverterTests
{
    public class ToMilliseconds
    {
        [Theory]
        // Worked example: 1h 2m 3s 4ms.
        [InlineData(1, 2, 3, 4, 3_723_004)]
        [InlineData(0, 1, 0, 0, 60_000)]
        [InlineData(1, 0, 0, 0, 3_600_000)]
        [InlineData(0, 0, 1, 0, 1_000)]
        [InlineData(0, 0, 0, 1, 1)]
        // Values above 59 are accepted: the numeric inputs in the UI enforce the per-field maximum,
        // the converter only adds up whatever it is handed.
        [InlineData(0, 90, 0, 0, 5_400_000)]
        [InlineData(0, 0, 90, 0, 90_000)]
        [InlineData(0, 0, 0, 1_500, 1_500)]
        public void WithPositiveParts_ReturnsSum(int hours, int minutes, int seconds, int milliseconds, int expected)
        {
            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        // Every part clamps to zero before the sum, so an all-negative input is also an empty value.
        [InlineData(-1, -1, -1, -1)]
        [InlineData(int.MinValue, int.MinValue, int.MinValue, int.MinValue)]
        public void WithZeroTotal_ReturnsNull(int hours, int minutes, int seconds, int milliseconds)
        {
            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().BeNull(
                "an empty duration must round-trip to NULL, not 0 - ProxySQL treats cache_ttl = 0 as 'do not cache'");
        }

        [Theory]
        // A negative part contributes 0; the remaining positive parts still count.
        [InlineData(-1, 2, 3, 4, 123_004)]
        [InlineData(1, -2, 3, 4, 3_603_004)]
        [InlineData(1, 2, -3, 4, 3_720_004)]
        [InlineData(1, 2, 3, -4, 3_723_000)]
        [InlineData(int.MinValue, 0, 0, 5, 5)]
        public void WithNegativeParts_ClampsThemToZero(int hours, int minutes, int seconds, int milliseconds, int expected)
        {
            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().Be(expected);
        }

        [Theory]
        // 596 h is the last hour count that fits in an int (2_145_600_000 ms); 597 h overflows.
        [InlineData(596, 0, 0, 0, 2_145_600_000)]
        [InlineData(596, 31, 23, 647, int.MaxValue)]
        public void JustBelowOverflow_ReturnsExactTotal(int hours, int minutes, int seconds, int milliseconds, int expected)
        {
            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(596, 31, 23, 648)]
        [InlineData(597, 0, 0, 0)]
        [InlineData(int.MaxValue, 0, 0, 0)]
        [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
        public void WhenTotalExceedsIntMaxValue_Saturates(int hours, int minutes, int seconds, int milliseconds)
        {
            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().Be(
                int.MaxValue,
                "the sum is accumulated as a long and capped, so an out-of-range duration clamps instead of throwing or wrapping");
        }
    }

    public class Decompose
    {
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void WithNullOrNonPositiveTotal_ReturnsAllZeros(int? totalMilliseconds)
        {
            var result = TimeInputConverter.Decompose(totalMilliseconds);

            result.Should().Be((0, 0, 0, 0));
        }

        [Theory]
        [InlineData(3_723_004, 1, 2, 3, 4)]
        [InlineData(1, 0, 0, 0, 1)]
        [InlineData(999, 0, 0, 0, 999)]
        [InlineData(1_000, 0, 0, 1, 0)]
        [InlineData(59_999, 0, 0, 59, 999)]
        [InlineData(60_000, 0, 1, 0, 0)]
        [InlineData(3_599_999, 0, 59, 59, 999)]
        [InlineData(3_600_000, 1, 0, 0, 0)]
        // Hours are not capped at 24 - the component shows a raw hour count.
        [InlineData(86_400_000, 24, 0, 0, 0)]
        [InlineData(int.MaxValue, 596, 31, 23, 647)]
        public void WithPositiveTotal_SplitsIntoParts(
            int totalMilliseconds,
            int expectedHours,
            int expectedMinutes,
            int expectedSeconds,
            int expectedMilliseconds)
        {
            var result = TimeInputConverter.Decompose(totalMilliseconds);

            result.Should().Be((expectedHours, expectedMinutes, expectedSeconds, expectedMilliseconds));
        }
    }

    public class RoundTrip
    {
        [Theory]
        [InlineData(1)]
        [InlineData(999)]
        [InlineData(1_000)]
        [InlineData(60_000)]
        [InlineData(3_723_004)]
        [InlineData(5_400_000)]
        [InlineData(86_400_000)]
        [InlineData(int.MaxValue)]
        public void DecomposeThenToMilliseconds_PreservesTheTotal(int totalMilliseconds)
        {
            var (hours, minutes, seconds, milliseconds) = TimeInputConverter.Decompose(totalMilliseconds);

            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().Be(totalMilliseconds);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-5)]
        public void DecomposeThenToMilliseconds_MapsEmptyTotalsToNull(int? totalMilliseconds)
        {
            var (hours, minutes, seconds, milliseconds) = TimeInputConverter.Decompose(totalMilliseconds);

            var result = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(1, 2, 3, 4)]
        [InlineData(0, 1, 0, 0)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(596, 31, 23, 647)]
        public void ToMillisecondsThenDecompose_PreservesTheParts(int hours, int minutes, int seconds, int milliseconds)
        {
            var total = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            TimeInputConverter.Decompose(total).Should().Be((hours, minutes, seconds, milliseconds));
        }

        [Theory]
        // Parts above their natural maximum are normalised by the round trip: 90 minutes comes back
        // as 1 h 30 m. The UI's numeric inputs prevent this, but the converter itself allows it.
        [InlineData(0, 90, 0, 0, 1, 30, 0, 0)]
        [InlineData(0, 0, 90, 0, 0, 1, 30, 0)]
        [InlineData(0, 0, 0, 1_500, 0, 0, 1, 500)]
        public void ToMillisecondsThenDecompose_NormalisesOversizedParts(
            int hours,
            int minutes,
            int seconds,
            int milliseconds,
            int expectedHours,
            int expectedMinutes,
            int expectedSeconds,
            int expectedMilliseconds)
        {
            var total = TimeInputConverter.ToMilliseconds(hours, minutes, seconds, milliseconds);

            TimeInputConverter.Decompose(total)
                .Should().Be((expectedHours, expectedMinutes, expectedSeconds, expectedMilliseconds));
        }
    }
}
