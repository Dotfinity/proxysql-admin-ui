using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxysqlAdminUi.Web.Migrations.AuthMigrations;

/// <summary>
/// Re-syncs the model snapshot with the current Identity model after the EF Core 10 upgrade.
/// The previous snapshot carried a stale "Username" property (the real column is "UserName"), which made
/// EF Core 9+ throw <c>PendingModelChangesWarning</c> from <c>MigrateAsync</c> and silently skipped default user seeding.
/// The schema itself is created by <c>EnsureCreatedAsync</c> from the model (see <c>DefaultUserSeedService</c>),
/// so this migration - like <see cref="InitialCreate"/> - intentionally contains no schema operations.
/// </summary>
public partial class EfCore10ModelUpdate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
