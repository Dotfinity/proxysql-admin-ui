using AwesomeAssertions;
using ProxysqlAdminUi.Web.Extensions;

namespace ProxysqlAdminUi.Tests.Extensions;

/// <summary>
/// Characterisation tests for <see cref="GlobalVariableFamily"/> - resolving the wrong
/// family makes a "LOAD ... TO RUNTIME" silently no-op, so the prefix match has to stay anchored at
/// the start of the name and stay case-insensitive. Every expectation below is what the
/// implementation produces today.
/// </summary>
public class GlobalVariableFamilyTests
{
    public class Resolve
    {
        [Theory]
        [InlineData("mysql-max_connections")]
        [InlineData("mysql-threads")]
        // The match is case-insensitive: ProxySQL lower-cases its own names, but a name typed into
        // the UI must resolve the same way.
        [InlineData("MYSQL-threads")]
        [InlineData("MySql-Threads")]
        // The prefix alone is enough - nothing requires a name after the dash.
        [InlineData("mysql-")]
        public void WithAMySqlPrefix_ReturnsTheMySqlFamily(string variableName)
        {
            GlobalVariableFamily.Resolve(variableName).Should().Be(GlobalVariableFamily.MySql);
        }

        [Theory]
        [InlineData("admin-web_enabled")]
        [InlineData("Admin-version")]
        [InlineData("ADMIN-ADMIN_CREDENTIALS")]
        [InlineData("admin-")]
        public void WithAnAdminPrefix_ReturnsTheAdminFamily(string variableName)
        {
            GlobalVariableFamily.Resolve(variableName).Should().Be(GlobalVariableFamily.Admin);
        }

        [Theory]
        // Families this UI cannot apply.
        [InlineData("sqliteserver-mysql_ifaces")]
        [InlineData("clickhouse-port")]
        [InlineData("ldap-enabled")]
        // No dash at all, so no family.
        [InlineData("monitor_username")]
        [InlineData("mysql")]
        [InlineData("admin")]
        public void WithAnUnsupportedOrPrefixlessName_ReturnsNull(string variableName)
        {
            GlobalVariableFamily.Resolve(variableName).Should().BeNull();
        }

        [Theory]
        // The match is anchored at index 0: a name that merely CONTAINS the prefix is not a member
        // of that family. "sqliteserver-mysql_ifaces" above is the real-world case this protects.
        [InlineData("xmysql-foo")]
        [InlineData("my-mysql-foo")]
        [InlineData("preadmin-foo")]
        // PINNED: the name is not trimmed, so a leading space defeats the match even though
        // IsNullOrWhiteSpace let the value through.
        [InlineData(" mysql-foo")]
        [InlineData("\tadmin-foo")]
        public void WithAPrefixThatIsNotAtTheStart_ReturnsNull(string variableName)
        {
            GlobalVariableFamily.Resolve(variableName).Should().BeNull(
                "the match is anchored - 'sqliteserver-mysql_ifaces' must not be loaded as a mysql variable");
        }

        [Theory]
        // Null and blank are absorbed rather than thrown: the grid binds this to a possibly-unset cell.
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void WithNullOrBlank_ReturnsNullWithoutThrowing(string? variableName)
        {
            GlobalVariableFamily.Resolve(variableName).Should().BeNull();
        }

        [Fact]
        public void FamilyConstants_MatchTheLoadAndSaveStatementSpelling()
        {
            GlobalVariableFamily.MySql.Should().Be("MYSQL VARIABLES");
            GlobalVariableFamily.Admin.Should().Be("ADMIN VARIABLES");
        }
    }

    public class IsEditable
    {
        [Theory]
        [InlineData("mysql-max_connections", true)]
        [InlineData("MYSQL-threads", true)]
        [InlineData("admin-web_enabled", true)]
        [InlineData("Admin-version", true)]
        [InlineData("sqliteserver-mysql_ifaces", false)]
        [InlineData("clickhouse-port", false)]
        [InlineData("ldap-enabled", false)]
        [InlineData("monitor_username", false)]
        [InlineData("xmysql-foo", false)]
        [InlineData(" mysql-foo", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void MirrorsResolve(string? variableName, bool expected)
        {
            GlobalVariableFamily.IsEditable(variableName).Should().Be(expected);
            GlobalVariableFamily.IsEditable(variableName)
                .Should().Be(GlobalVariableFamily.Resolve(variableName) is not null);
        }
    }

    public class PrefixOf
    {
        [Theory]
        // Everything up to and including the first dash, with the original casing preserved.
        [InlineData("mysql-max_connections", "mysql-")]
        [InlineData("MYSQL-threads", "MYSQL-")]
        [InlineData("clickhouse-port", "clickhouse-")]
        // Only the FIRST dash counts, even when the remainder holds more.
        [InlineData("mysql-connection-max_age", "mysql-")]
        // A leading dash yields just the dash.
        [InlineData("-orphan", "-")]
        [InlineData("mysql-", "mysql-")]
        public void WithADashedName_ReturnsThePrefixIncludingTheDash(string variableName, string expected)
        {
            GlobalVariableFamily.PrefixOf(variableName).Should().Be(expected);
        }

        [Theory]
        // No dash means no prefix to strip, so the whole name comes back.
        [InlineData("monitor_username", "monitor_username")]
        [InlineData("mysql", "mysql")]
        public void WithoutADash_ReturnsTheWholeName(string variableName, string expected)
        {
            GlobalVariableFamily.PrefixOf(variableName).Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithNullOrBlank_ReturnsEmpty(string? variableName)
        {
            GlobalVariableFamily.PrefixOf(variableName).Should().BeEmpty();
        }

        [Fact]
        public void WithALeadingSpace_KeepsTheSpaceInThePrefix()
        {
            // PINNED: nothing trims, so the untrimmed name leaks into the read-only message.
            GlobalVariableFamily.PrefixOf(" mysql-foo").Should().Be(" mysql-");
        }
    }

    public class NotEditableMessage
    {
        [Theory]
        [InlineData("clickhouse-port", "Variables with prefix 'clickhouse-' cannot be edited from this UI.")]
        [InlineData("sqliteserver-mysql_ifaces", "Variables with prefix 'sqliteserver-' cannot be edited from this UI.")]
        [InlineData("ldap-enabled", "Variables with prefix 'ldap-' cannot be edited from this UI.")]
        // A prefixless name falls back to the whole name, which reads oddly but is the current text.
        [InlineData("monitor_username", "Variables with prefix 'monitor_username' cannot be edited from this UI.")]
        // Blank and null both degrade to an empty prefix rather than throwing.
        [InlineData("", "Variables with prefix '' cannot be edited from this UI.")]
        [InlineData(null, "Variables with prefix '' cannot be edited from this UI.")]
        public void DescribesThePrefixThatBlockedTheEdit(string? variableName, string expected)
        {
            GlobalVariableFamily.NotEditableMessage(variableName).Should().Be(expected);
        }
    }
}
