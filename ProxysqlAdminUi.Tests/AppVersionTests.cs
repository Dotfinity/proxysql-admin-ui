using AwesomeAssertions;
using ProxysqlAdminUi.Web;

namespace ProxysqlAdminUi.Tests;

/// <summary>
/// Covers <see cref="AppVersion"/> - the type reads <c>BUILD_VERSION</c>/<c>BUILD_SUFFIX</c>
/// from the process environment in its constructor, so every test below mutates process-wide state
/// and restores it in a <c>finally</c>. They all live in one class on purpose: xunit runs the tests
/// of a single class sequentially, which - together with the assembly-wide
/// <c>[Parallelization(Mode = ParallelMode.None)]</c> in AssemblyInfo.cs - keeps the two variables
/// from being read by one test while another is writing them.
/// </summary>
public class AppVersionTests
{
    private const string BuildVersionVariable = "BUILD_VERSION";
    private const string BuildSuffixVariable = "BUILD_SUFFIX";

    [Theory]
    [InlineData("2025.11.04", "123456", "2025.11.04-123456")]
    [InlineData("1.0.0", "1", "1.0.0-1")]
    [InlineData(" 2025.11.04 ", " 123456 ", "2025.11.04-123456")]
    public void Version_WithBuildVersionAndSuffix_CombinesThemAndIsRelease(
        string buildVersion,
        string buildSuffix,
        string expectedVersion)
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, buildVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, buildSuffix);

            var sut = new AppVersion();

            sut.Version.Should().Be(expectedVersion);
            sut.IsRelease.Should().BeTrue(
                "only CI stamps both BUILD_VERSION and BUILD_SUFFIX, and only those builds match a published GitHub release tag");
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Version_WithBuildVersionButNoSuffix_ShowsVersionAloneAndIsNotRelease(string? buildSuffix)
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, "2025.11.04");
            Environment.SetEnvironmentVariable(BuildSuffixVariable, buildSuffix);

            var sut = new AppVersion();

            sut.Version.Should().Be("2025.11.04", "a manual build-arg build must not render a trailing dash");
            sut.IsRelease.Should().BeFalse("a version without the run-id suffix cannot match a release tag");
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }

    [Fact]
    public void Version_WithWhitespaceBuildVersion_IsTreatedAsUnset()
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, "   ");
            Environment.SetEnvironmentVariable(BuildSuffixVariable, "123456");

            var sut = new AppVersion();

            sut.Version.Should().NotBeNullOrWhiteSpace();
            sut.Version.Should().NotContain("123456", "a blank BUILD_VERSION must fall back to the assembly version");
            sut.IsRelease.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }

    [Theory]
    // null and "" are indistinguishable here: .NET deletes the variable for either value. Both
    // spellings are exercised so the test still holds if that ever changes.
    [InlineData(null)]
    [InlineData("")]
    public void Version_WithoutBuildVersion_FallsBackToTheAssemblyVersionAndIsNotRelease(string? buildVersion)
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, buildVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, "123456");

            var sut = new AppVersion();

            // The entry assembly under test is the test host, so its informational version is not a
            // fixed string; assert the contract instead - a non-empty value with the SDK's "+sha"
            // source-revision suffix stripped, or the "dev" fallback.
            sut.Version.Should().NotBeNullOrWhiteSpace();
            sut.Version.Should().NotContain("+", "the footer shows a version, not a source-revision id");
            sut.IsRelease.Should().BeFalse("a local or dev build has no GitHub release to link to");
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }

    [Fact]
    public void Version_WithoutBuildVersion_IgnoresBuildSuffix()
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, null);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, "999999");

            var sut = new AppVersion();

            sut.Version.Should().NotContain("999999", "BUILD_SUFFIX is only meaningful together with BUILD_VERSION");
            sut.IsRelease.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }

    [Fact]
    public void Version_IsComputedOnceInTheConstructor_SoLaterEnvironmentChangesAreIgnored()
    {
        var previousVersion = Environment.GetEnvironmentVariable(BuildVersionVariable);
        var previousSuffix = Environment.GetEnvironmentVariable(BuildSuffixVariable);

        try
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, "2025.11.04");
            Environment.SetEnvironmentVariable(BuildSuffixVariable, "123456");

            var sut = new AppVersion();

            Environment.SetEnvironmentVariable(BuildVersionVariable, "2099.01.01");
            Environment.SetEnvironmentVariable(BuildSuffixVariable, "999999");

            sut.Version.Should().Be(
                "2025.11.04-123456",
                "AppVersion is registered as a singleton and snapshots the environment at construction");
            sut.IsRelease.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(BuildVersionVariable, previousVersion);
            Environment.SetEnvironmentVariable(BuildSuffixVariable, previousSuffix);
        }
    }
}
