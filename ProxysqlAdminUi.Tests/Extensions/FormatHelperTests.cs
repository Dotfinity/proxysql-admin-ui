using System.Globalization;
using AwesomeAssertions;
using ProxysqlAdminUi.Web.Extensions;

namespace ProxysqlAdminUi.Tests.Extensions;

/// <summary>
/// Characterisation tests for <see cref="FormatHelper"/>. Every expectation below is the output the
/// implementation produces today (the boundaries are pinned, not redesigned), so
/// a failure here means somebody changed a user-visible format - decide deliberately, then update.
/// </summary>
public class FormatHelperTests
{
    /// <summary>
    /// Several helpers format through <c>"0.##"</c>, whose decimal separator comes from
    /// <see cref="CultureInfo.CurrentCulture"/> - on a machine running e.g. bg-BG the dashboard shows
    /// "228,45 MB". Pinning the culture per test lets the expected strings stay literal and keeps the
    /// suite green on any developer machine and in CI.
    /// </summary>
    private static IDisposable InvariantCulture() => new CultureScope(CultureInfo.InvariantCulture);

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previous = CultureInfo.CurrentCulture;

        public CultureScope(CultureInfo culture) => CultureInfo.CurrentCulture = culture;

        public void Dispose() => CultureInfo.CurrentCulture = _previous;
    }

    public class FormatBytes
    {
        [Theory]
        // Unit boundaries: the unit flips exactly at each 1024 multiple.
        [InlineData(0L, "0 B")]
        [InlineData(1L, "1 B")]
        [InlineData(1023L, "1023 B")]
        [InlineData(1024L, "1 KB")]
        // 1048575 B is one byte short of a megabyte: it stays in KB, but "0.##" rounds the mantissa
        // (1023.999...) up to a full 1024, so the display reads "1024 KB" rather than "1 MB".
        [InlineData(1_048_575L, "1024 KB")]
        [InlineData(1_048_576L, "1 MB")]
        [InlineData(1_073_741_823L, "1024 MB")]
        [InlineData(1_073_741_824L, "1 GB")]
        [InlineData(1_099_511_627_775L, "1024 GB")]
        [InlineData(1_099_511_627_776L, "1 TB")]
        // TB is the last unit, so anything larger keeps scaling the number instead of the suffix.
        [InlineData(1_125_899_906_842_624L, "1024 TB")]
        [InlineData(long.MaxValue, "8388608 TB")]
        public void WithBoundaryValues_ReturnsExpectedUnitAndRounding(long bytes, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatBytes(bytes).Should().Be(expected);
        }

        [Theory]
        [InlineData(1536L, "1.5 KB")]
        [InlineData(1126L, "1.1 KB")]
        [InlineData(1229L, "1.2 KB")]
        [InlineData(1_572_864L, "1.5 MB")]
        [InlineData(239_542_272L, "228.45 MB")]
        public void WithFractionalValues_RoundsToTwoDecimalsAndTrimsTrailingZeros(long bytes, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatBytes(bytes).Should().Be(expected);
        }

        [Theory]
        // Negative input never enters the scaling loop (the >= 1024 guard fails immediately), so it is
        // always reported in bytes regardless of magnitude.
        [InlineData(-1L, "-1 B")]
        [InlineData(-1024L, "-1024 B")]
        // The byte count is widened to double before formatting, so magnitudes beyond 2^53 are
        // printed with the precision of a double, not of the original long.
        [InlineData(long.MinValue, "-9223372036854780000 B")]
        public void WithNegativeValues_StaysInBytes(long bytes, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatBytes(bytes).Should().Be(expected);
        }

        [Fact]
        public void UnderNonInvariantCulture_UsesTheCultureDecimalSeparator()
        {
            using var _ = new CultureScope(CultureInfo.GetCultureInfo("de-DE"));

            FormatHelper.FormatBytes(1536L).Should().Be(
                "1,5 KB",
                "the helper formats with the ambient culture - callers that compare the string "
                + "must not assume a dot");
        }
    }

    public class FormatUptime
    {
        [Theory]
        // Day/hour/minute boundaries.
        [InlineData(0L, "0d 0h 0m")]
        [InlineData(59L, "0d 0h 0m")]
        [InlineData(60L, "0d 0h 1m")]
        [InlineData(3_599L, "0d 0h 59m")]
        [InlineData(3_600L, "0d 1h 0m")]
        [InlineData(86_399L, "0d 23h 59m")]
        [InlineData(86_400L, "1d 0h 0m")]
        [InlineData(90_061L, "1d 1h 1m")]
        [InlineData(31_536_000L, "365d 0h 0m")]
        public void WithBoundaryValues_ReturnsDayHourMinuteBreakdown(long seconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatUptime(seconds).Should().Be(expected);
        }

        [Theory]
        // Seconds are truncated, never rounded: 119s is one minute, not two.
        [InlineData(119L, "0d 0h 1m")]
        [InlineData(120L, "0d 0h 2m")]
        public void WithPartialMinutes_TruncatesTowardsZero(long seconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatUptime(seconds).Should().Be(expected);
        }

        [Theory]
        // Uptime is never negative in practice; pinned so a future change to the contract is deliberate.
        [InlineData(-1L, "0d 0h 0m")]
        [InlineData(-60L, "0d 0h -1m")]
        [InlineData(-90_061L, "-1d -1h -1m")]
        public void WithNegativeSeconds_EmitsNegativeComponents(long seconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatUptime(seconds).Should().Be(expected);
        }
    }

    public class FormatLargeNumber
    {
        [Theory]
        // Suffix boundaries: the suffix flips exactly once at each power of 1000.
        [InlineData(0L, "0")]
        [InlineData(1L, "1")]
        [InlineData(999L, "999")]
        [InlineData(1_000L, "1K")]
        // 999999 is below the megabyte-equivalent threshold, so it keeps the K suffix - but "0.##"
        // rounds 999.999 up to 1000, so the display reads "1000K" rather than "1M".
        [InlineData(999_999L, "1000K")]
        [InlineData(1_000_000L, "1M")]
        [InlineData(999_999_999L, "1000M")]
        [InlineData(1_000_000_000L, "1B")]
        // B is the last suffix, so larger values keep scaling the number.
        [InlineData(1_000_000_000_000L, "1000B")]
        [InlineData(long.MaxValue, "9223372036.85B")]
        public void WithBoundaryValues_FlipsSuffixExactlyOnce(long number, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatLargeNumber(number).Should().Be(expected);
        }

        [Theory]
        [InlineData(1_500L, "1.5K")]
        [InlineData(1_234L, "1.23K")]
        [InlineData(1_235L, "1.24K")]
        [InlineData(1_500_000L, "1.5M")]
        [InlineData(2_500_000_000L, "2.5B")]
        public void WithFractionalValues_RoundsToTwoDecimals(long number, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatLargeNumber(number).Should().Be(expected);
        }

        [Theory]
        // Negative input fails every >= threshold, so it is printed verbatim without a suffix.
        [InlineData(-999L, "-999")]
        [InlineData(-1_000L, "-1000")]
        [InlineData(-1_000_000_000L, "-1000000000")]
        public void WithNegativeValues_ReturnsUnsuffixedNumber(long number, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatLargeNumber(number).Should().Be(expected);
        }
    }

    public class FormatMiliseconds
    {
        [Theory]
        // Seconds-only branch.
        [InlineData(0L, "00.000")]
        [InlineData(1L, "00.001")]
        [InlineData(999L, "00.999")]
        [InlineData(1_000L, "01.000")]
        [InlineData(59_999L, "59.999")]
        // Minute threshold: the mm component appears.
        [InlineData(60_000L, "01:00.000")]
        [InlineData(3_599_999L, "59:59.999")]
        // Hour threshold: the hh component appears.
        [InlineData(3_600_000L, "01:00:00.000")]
        [InlineData(86_399_999L, "23:59:59.999")]
        // Day threshold: a "Nd " prefix appears and hh restarts at 00.
        [InlineData(86_400_000L, "1d 00:00:00.000")]
        [InlineData(90_061_001L, "1d 01:01:01.001")]
        [InlineData(172_800_000L, "2d 00:00:00.000")]
        public void AtUnitThresholds_AddsExactlyOneComponent(long milliseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMiliseconds(milliseconds).Should().Be(expected);
        }

        [Theory]
        // Sub-millisecond precision does not exist in the input, but the fff field is fixed-width.
        [InlineData(12L, "00.012")]
        [InlineData(1_234L, "01.234")]
        public void WithSubSecondValues_PadsTheMillisecondField(long milliseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMiliseconds(milliseconds).Should().Be(expected);
        }

        [Theory]
        // BUG pinned: the branch chain tests ts.Days/Hours/Minutes > 0, which is never true for a
        // negative TimeSpan, so every negative duration falls through to the seconds-only branch.
        // The output therefore loses both the sign and every component above seconds: -1d 1h 1m
        // 1.001s renders as "01.001" and a flat -1h renders as "00.000".
        [InlineData(-1_500L, "01.500")]
        [InlineData(-90_061_001L, "01.001")]
        [InlineData(-3_600_000L, "00.000")]
        public void WithNegativeValues_CollapsesToTheSecondsComponent(long milliseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMiliseconds(milliseconds).Should().Be(expected);
        }
    }

    public class FormatMicroseconds
    {
        [Theory]
        // Seconds-only branch.
        [InlineData(0L, "00.000000")]
        [InlineData(1L, "00.000001")]
        [InlineData(999L, "00.000999")]
        [InlineData(1_000L, "00.001000")]
        [InlineData(999_999L, "00.999999")]
        [InlineData(1_000_000L, "01.000000")]
        [InlineData(59_999_999L, "59.999999")]
        // Minute threshold.
        [InlineData(60_000_000L, "01:00.000000")]
        [InlineData(3_599_999_999L, "59:59.999999")]
        // Hour threshold.
        [InlineData(3_600_000_000L, "01:00:00.000000")]
        [InlineData(86_399_999_999L, "23:59:59.999999")]
        // Day threshold.
        [InlineData(86_400_000_000L, "1d 00:00:00.000000")]
        [InlineData(90_061_001_002L, "1d 01:01:01.001002")]
        public void AtUnitThresholds_AddsExactlyOneComponent(long microseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMicroseconds(microseconds).Should().Be(expected);
        }

        [Theory]
        // TimeSpan ticks are 100ns, so a whole number of microseconds is always exact - no rounding.
        [InlineData(1_234_567L, "01.234567")]
        [InlineData(7L, "00.000007")]
        public void WithSubSecondValues_KeepsFullMicrosecondPrecision(long microseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMicroseconds(microseconds).Should().Be(expected);
        }

        [Theory]
        // Same fall-through bug as FormatMiliseconds: a negative duration keeps only its (unsigned)
        // seconds component, so a whole negative hour is displayed as "00.000000".
        [InlineData(-1_500_000L, "01.500000")]
        [InlineData(-90_061_001_002L, "01.001002")]
        [InlineData(-3_600_000_000L, "00.000000")]
        public void WithNegativeValues_CollapsesToTheSecondsComponent(long microseconds, string expected)
        {
            using var _ = InvariantCulture();

            FormatHelper.FormatMicroseconds(microseconds).Should().Be(expected);
        }
    }

    /// <summary>
    /// The query-digest grid shortens long statements to a fixed width. This method does
    /// no culture-sensitive formatting, so it needs no culture scope.
    /// </summary>
    public class TruncateDigestText
    {
        [Theory]
        // Nothing to show is an empty string, never null - the grid binds the result directly.
        [InlineData(null, "")]
        [InlineData("", "")]
        public void WithNullOrEmpty_ReturnsEmptyString(string? text, string expected)
        {
            FormatHelper.TruncateDigestText(text).Should().Be(expected);
        }

        [Theory]
        // Whitespace is not treated as empty - only null and "" short-circuit.
        [InlineData(" ", " ")]
        [InlineData("SELECT 1", "SELECT 1")]
        [InlineData("SELECT * FROM users WHERE id=?", "SELECT * FROM users WHERE id=?")]
        public void WithTextUpToTheDefaultLimit_ReturnsItUnchanged(string text, string expected)
        {
            FormatHelper.TruncateDigestText(text).Should().Be(expected);
        }

        [Fact]
        public void AtExactlyTheDefaultLimit_ReturnsTheTextUnchanged()
        {
            var text = new string('a', 30);

            FormatHelper.TruncateDigestText(text).Should().Be(
                text,
                "the comparison is > maxLength, so a string of exactly maxLength characters is not truncated");
        }

        [Fact]
        public void OneCharacterOverTheDefaultLimit_KeepsTheFirstThirtyAndAppendsAnEllipsis()
        {
            var text = new string('a', 31);

            FormatHelper.TruncateDigestText(text).Should().Be(
                new string('a', 30) + "...",
                "the ellipsis is three literal dots appended after maxLength characters, so the "
                + "returned string is maxLength + 3 characters long");
        }

        [Theory]
        // The ellipsis is three ASCII dots, not the single U+2026 character.
        [InlineData("abcdefg", 5, "abcde...")]
        [InlineData("abcdefg", 6, "abcdef...")]
        [InlineData("abcdefg", 7, "abcdefg")]
        [InlineData("abcdefg", 8, "abcdefg")]
        [InlineData("abcdefg", 1, "a...")]
        // maxLength 0 keeps nothing but still emits the ellipsis for non-empty input.
        [InlineData("abcdefg", 0, "...")]
        // ... while empty input short-circuits before the ellipsis is ever added.
        [InlineData("", 0, "")]
        public void WithACustomMaxLength_TruncatesAtThatLength(string text, int maxLength, string expected)
        {
            FormatHelper.TruncateDigestText(text, maxLength).Should().Be(expected);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void WithANegativeMaxLength_Throws(int maxLength)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => FormatHelper.TruncateDigestText("SELECT 1", maxLength));
        }

        [Fact]
        public void WithANegativeMaxLengthAndNullText_StillThrows()
        {
            // The guard runs before the null/empty short-circuit, so null text does not excuse it.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => FormatHelper.TruncateDigestText(null, -1));
        }

        [Fact]
        public void WithAstralCharacters_CutsByUtf16UnitAndCanSplitASurrogatePair()
        {
            // PINNED: truncation counts UTF-16 code units, so a cut that lands inside a surrogate
            // pair leaves a lone high surrogate. Harmless for ASCII SQL, but worth knowing if a
            // digest ever carries an emoji in a string literal.
            const string text = "\U0001F600x";

            FormatHelper.TruncateDigestText(text, 1).Should().Be("\uD83D...");
        }
    }
}
