# ProxySQL Admin UI - AI Assistant Context

## Project Overview

**ProxySQL Admin UI** is a modern web-based administration dashboard for managing ProxySQL (a high-performance MySQL proxy server). Built with .NET 10 and Blazor Server, it provides a user-friendly interface for monitoring performance, configuring backend servers, managing user credentials, and defining query routing rules.

**License:** MIT
**Primary Language:** C# (.NET 10)
**Web Framework:** Blazor Server with Radzen UI Components

## Quick Facts

- **Main Project:** `ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj`
- **Startup File:** `ProxysqlAdminUi.Web/Program.cs`
- **Entry Point:** ASP.NET Core web server on port 8000 (configurable)
- **Deployment:** Docker containers (Alpine Linux based)
- **Databases:**
  - ProxySQL **admin interface** (MySQL protocol, port 6032 in `docker/proxysql.cnf`; the admin tables such as `mysql_servers` only exist there, not on the 6033 client interface)
  - SQLite (local identity/authentication)

## Technology Stack

### Core Framework
- **.NET 10** - LTS version with Alpine Linux runtime (upgraded from .NET 8 in September 2026)
- **ASP.NET Core** - Web framework
- **Blazor Server** - Interactive server-side rendering
- **Entity Framework Core 10.0.x** - ORM for data access

### UI & Frontend
- **Radzen Blazor Components 11.x** - Material Design component library (`<RadzenTheme Theme="material" />` in `App.razor`)
- **Material Design Icons** - Icon set
- **Interactive Server Rendering** - Real-time UI updates via SignalR

### Data Access
- **MySql.EntityFrameworkCore (Oracle, 10.0.x)** - ProxySQL connection via MySQL protocol. Pinned to the release matching the EF Core major; do **not** switch to Pomelo (unmaintained).
- **Microsoft.EntityFrameworkCore.Sqlite** - Local authentication database
- **Direct SQL Execution** - For complex ProxySQL-specific operations (`FromSqlRaw` / `ExecuteSqlRawAsync`)

### Supporting Libraries
- **Newtonsoft.Json 13.0.x** - JSON serialization
- **ASP.NET Core Identity** - Cookie-based authentication

> Swagger/OpenAPI (`Swashbuckle.AspNetCore`) was removed: there are no API controllers to document.
> Do not re-add it unless a real API surface lands.

### Testing
- **xunit.v3 4.x** on **Microsoft.Testing.Platform** (runner selected by the root `global.json`)
- **Microsoft.AspNetCore.Mvc.Testing** - `WebApplicationFactory<Program>` integration/smoke tests
- **AwesomeAssertions** - Fluent assertions
- **coverlet.collector** - Code coverage

## Architecture

### Layered Architecture

```
┌─────────────────────────────────────┐
│  UI Layer (Blazor Components)       │
│  - Pages/*.razor                    │
│  - Layout/MainLayout.razor          │
│  - CustomComponents/                │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Service Layer                      │
│  - DefaultUserSeedService.cs        │
│  - Business logic in components     │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Repository Layer                   │
│  - ProxySqlRepository.cs            │
│  - Data access methods              │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Data Layer (EF Core)               │
│  - ProxySqlContext.cs               │
│  - ProxysqlAdminUiWebAuthContext.cs │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  Databases                          │
│  - ProxySQL (MySQL)                 │
│  - SQLite (Identity)                │
└─────────────────────────────────────┘
```

### Key Patterns
- **Repository Pattern** - `ProxySqlRepository.cs` abstracts data access
- **DbContext Pattern** - Separate contexts for different data sources
- **Dependency Injection** - Configured in `Program.cs`
- **Component-Based UI** - Blazor Server components with code-behind

## Directory Structure

```
ProxysqlAdminUi.Web/
├── Components/
│   ├── Account/              # Authentication UI (Login, Logout, Lockout, Manage/*) - no registration
│   ├── Pages/                # Main application pages
│   │   ├── Home.razor        # Dashboard with metrics
│   │   ├── MySql/            # MySQL management pages
│   │   │   ├── Servers/      # Backend server management
│   │   │   ├── Users/        # User credentials
│   │   │   ├── Rules/        # Query rules
│   │   │   └── Queries/      # Query statistics
│   │   └── ProxySQL/         # ProxySQL configuration pages
│   ├── CustomComponents/     # Reusable components (e.g., TimeInput)
│   └── Layout/               # Layout components (MainLayout, NavMenu)
├── Contexts/
│   ├── ProxySqlContext.cs               # EF Core context for ProxySQL
│   └── ProxysqlAdminUiWebAuthContext.cs # EF Core context for Identity
├── Data/                     # Identity and data utilities
├── Models/                   # Entity models (mapped to database tables)
├── Repositories/
│   └── ProxySqlRepository.cs # Data access layer
├── Security/
│   ├── PaiClaims.cs                    # Custom Identity claim types
│   └── MustChangePasswordMiddleware.cs # Forces the seeded admin off the default password
├── Services/
│   └── DefaultUserSeedService.cs # User initialization
├── Extensions/
│   └── FormatHelper.cs       # Utility methods for formatting
├── ViewModel/                # View models for UI
├── Migrations/               # EF Core migrations
├── wwwroot/                  # Static files
├── Program.cs                # Application startup
├── App.razor                 # Root component
├── Routes.razor              # Routing configuration
├── _Imports.razor            # Global using directives
└── appsettings.json          # Configuration
```

## Key Files

### Application Startup & Configuration
- **`Program.cs`** - Core application setup:
  - Loads `appsettings.json`, `appsettings.{Environment}.json`, then env vars (`PAI_` prefix first, then unprefixed; env vars win)
  - Registers services (Blazor, Radzen, Identity, DbContexts, Repository)
  - Configures authentication (cookie-based)
  - Runs `DefaultUserSeedService` at startup (SQLite schema + default admin)
  - Calls `UseAuthentication()` / `UseAuthorization()` explicitly, then `UseMustChangePassword()` (see below), then `UseAntiforgery()`
  - No Swagger/OpenAPI and no output caching - both were removed as unused (there are no API controllers)
  - Ends with `public partial class Program;` so tests can boot the app via `WebApplicationFactory<Program>`

- **`appsettings.json`** - Configuration:
  - ProxySQL connection string
  - Default admin credentials
  - Kestrel HTTP server settings
  - Logging configuration

### Data Access
- **`Contexts/ProxySqlContext.cs`** - ProxySQL database context:
  - Maps to ProxySQL tables (mysql_servers, mysql_users, mysql_query_rules, stats_*)
  - Configured for MySQL connection

- **`Repositories/ProxySqlRepository.cs`** - Data access layer:
  - CRUD operations for servers, users, query rules
  - Statistics retrieval methods
  - Direct SQL execution for complex queries
  - Methods: `GetServers()`, `GetUsers()`, `GetQueryRules()`, `GetQueryDigest()`, etc.

### Models (Entity Classes)
- **`MysqlServerModel.cs`** - Backend MySQL server configuration
- **`MysqlUserModel.cs`** - User credentials
- **`MysqlQueryRuleModel.cs`** - Query routing/caching rules (38 properties)
- **`GlobalVariableModel.cs`** - ProxySQL configuration variables
- **Statistics Models:**
  - `StatsMySqlGlobalModel.cs` - Global statistics
  - `StatsMysqlQueryDigestModel.cs` - Query statistics
  - `StatsMemoryMetricsModel.cs` - Memory usage

### UI Components
- **`Components/Pages/Home.razor`** (252 lines) - Main dashboard:
  - 8 metric cards: cache efficiency, memory, health, servers, connections, traffic, uptime
  - Uses Radzen gauges, cards, and charts

- **`Components/Layout/MainLayout.razor`** - Application layout:
  - Top navigation menu
  - 6 main sections: Home, Users, Servers, Rules, Queries, Variables
  - Footer with version badge

### Utilities
- **`Extensions/FormatHelper.cs`** - Formatting utilities:
  - `FormatBytes()` - Human-readable byte sizes (KB, MB, GB)
  - `FormatUptime()` - Converts seconds to days/hours/minutes
  - `FormatLargeNumber()` - Number abbreviation (K, M, B)
  - `FormatMicroseconds()` - Precision time formatting

## Database Information

### ProxySQL Database (MySQL Protocol)
**Connection:** Configured in `appsettings.json`
- **Host:** localhost (or `proxysql` in Docker)
- **Port:** 6032 (ProxySQL admin interface; `mysql_ifaces` in `docker/proxysql.cnf`). The shipped `appsettings.json` default still says 6033 - override it via `PAI_ConnectionStrings__ProxySqlContext`.
- **User:** radmin
- **Password:** radmin

**Tables:**
- `mysql_servers` - Backend server configuration
- `mysql_users` - User credentials
- `mysql_query_rules` - Query routing rules
- `stats_mysql_query_digest` - Query statistics
- `stats_mysql_global` - Global metrics
- `stats_memory_metrics` - Memory usage
- `global_variables` - Configuration variables

### Identity Database (SQLite)
**Location:** `/app/db/app.db` (Docker) or `./db/app.db` (local); override the folder with `APP_DB_PATH`
- **Provider:** SQLite via Entity Framework Core
- **Purpose:** ASP.NET Core Identity (user authentication)
- **Tables:** AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
- **Migrations:** Located in `Migrations/AuthMigrations/`
- **Schema pattern (important):** `DefaultUserSeedService` calls `EnsureCreatedAsync()` (creates the schema from the model) and then `MigrateAsync()`. The checked-in migrations (`InitialCreate`, `EfCore10ModelUpdate`) therefore have **empty** `Up`/`Down` bodies and only keep the model snapshot in sync. EF Core 9+ throws `PendingModelChangesWarning` from `MigrateAsync` when the snapshot drifts from the model; `SqliteDbExtensions` therefore ignores that warning (otherwise the seed service's catch-all swallowed it and no default admin user was created). Still add a new migration whenever the Identity model changes so the snapshot stays current.

## Development Workflow

### Local Development
```bash
# Run with hot reload
dotnet watch --project ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj

# Build
dotnet build

# Run tests (Microsoft.Testing.Platform mode: use no argument, --solution or --project;
# a positional project path is ignored on the .NET 10 SDK and reports "zero tests ran")
dotnet test
dotnet test --project ProxysqlAdminUi.Tests/ProxysqlAdminUi.Tests.csproj

# Access at: http://localhost:5203
```

### Docker Development Environment
```bash
# Start all services (app, ProxySQL, MariaDB)
cd docker/
docker-compose up -d

# Access:
# - App: http://localhost:8000
# - ProxySQL Admin: http://localhost:6080
# - MariaDB: localhost:3306
```

### Adding New Pages
1. Create `.razor` file in `Components/Pages/` or appropriate subdirectory
2. Add `@page "/route"` directive at the top
3. Add `@attribute [Authorize]` for protected pages
4. Add navigation link in `MainLayout.razor`
5. Use Radzen components for UI consistency

### Database Changes
1. Modify entity model in `Models/`
2. Update `ProxySqlContext.cs` if needed
3. Add/update repository methods in `ProxySqlRepository.cs`
4. For Identity changes, run migration:
   ```bash
   dotnet ef migrations add MigrationName --context ProxysqlAdminUiWebAuthContext
   ```

## Common Tasks

### Adding a New ProxySQL Feature
1. **Create Model** - Add entity class in `Models/`
2. **Update Context** - Add `DbSet<T>` to `ProxySqlContext.cs`
3. **Add Repository Methods** - Implement CRUD in `ProxySqlRepository.cs`
4. **Create Page Component** - Add `.razor` file in `Components/Pages/`
5. **Update Navigation** - Add menu item in `MainLayout.razor`

### Adding Statistics/Metrics
1. Create model class in `Models/` (e.g., `StatsMyNewMetricModel.cs`)
2. Add query method in `ProxySqlRepository.cs`
3. Update dashboard (`Home.razor`) or create dedicated page
4. Use Radzen charts/gauges for visualization

### Modifying Authentication
- **User Model:** `Data/ProxysqlAdminUiWebUser.cs`
- **DbContext:** `Contexts/ProxysqlAdminUiWebAuthContext.cs`
- **Account Pages:** `Components/Account/` (Login, Lockout, Logout, AccessDenied, InvalidUser, 2FA, `Manage/*`)
- **Configuration:** `Program.cs` - Identity services registration

**Self-registration is closed.** `Register`, `ResendEmailConfirmation`, `ConfirmEmail*`, the whole
forgot/reset-password flow, `ExternalLogin*` and `Manage/Email` are deleted, along with
`IdentityNoOpEmailSender` - there is no e-mail sender and no external provider. Accounts come from
`DefaultUsers` only. `/Account/Register` is a 404. A lost admin password is recovered by deleting the
identity database (see README, "Recovering a lost admin password").

**Signing out** is a statically rendered page, `Components/Account/Pages/Logout.razor`. It cannot live
in `MainLayout`: `App.razor` renders the app with `new InteractiveServerRenderMode(false)` (no
prerendering), so an `<AntiforgeryToken />` in the layout has no token and the POST is rejected.
Everything under `/Account` is SSR, so the Blazor SSR form there gets its token automatically. The old
minimal-API `MapPost("/Logout")` was removed to avoid an ambiguous route.

**Forced password change.** While the seeded account still authenticates with its configured password,
`DefaultUserSeedService` adds the user claim `PaiClaims.MustChangePassword` (`"pai:must_change_password"`,
value `"true"`, see `Security/PaiClaims.cs`), and `Security/MustChangePasswordMiddleware` redirects every
request to `/Account/Manage/ChangePassword`. This is a **claim, not a column** - deliberately, so it needs
no Identity schema change (the schema comes from `EnsureCreatedAsync` and the migrations are empty).
The default `UserClaimsPrincipalFactory` carries the claim in the auth cookie, so the check costs no
database round-trip. `ChangePassword.razor` removes the claim and calls `RefreshSignInAsync` (in that
order - refreshing first would rebuild the cookie with the claim still attached). The check re-runs on
every startup, so a deployment already live on the documented default password is caught too.

**Lockout** is on: 5 failed attempts, 5 minutes (`Program.cs`), with `lockoutOnFailure: true` in
`Login.razor`. With no password-reset flow, a locked-out sole admin simply waits out the window.

## Build & Deployment

### Docker Build
**Dockerfile:** `docker/Dockerfile` (multi-stage build)
- **Stage 1:** Build with .NET SDK 10.0 Alpine (self-contained publish for `linux-musl-x64`)
- **Stage 2:** Runtime with ASP.NET Core 10.0 Alpine + full ICU
- **Output:** Alpine-based image (~460MB)

```bash
docker build -f docker/Dockerfile \
  --build-arg BUILD_VERSION=2025.11.04 \
  --build-arg BUILD_SUFFIX=12345 \
  -t dotfinity/proxysql-admin-ui:latest .
```

### CI/CD Pipelines (GitHub Actions)

**Development Branch (`dev`):**
- Workflow: `.github/workflows/docker-container-builder.yml`
- Registry: Private (proget.dotfinity.eu)
- Tags: `{DATE}-{RUN_ID}`, `latest`

**Main Branch (`main`):**
- Workflow: `.github/workflows/docker-container-publish.yml`
- Registry: Docker Hub (dotfinity/proxysql-admin-ui)
- Tags: `{DATE}-{RUN_ID}`, `latest`
- Creates git tag and pushes to repository

## Environment Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Development/Staging/Production
- `ASPNETCORE_URLS` - Server binding (default: http://+:8000)
- `PAI_ConnectionStrings__ProxySqlContext` - ProxySQL connection string
- `PAI_DefaultUsers__0__Username` / `PAI_DefaultUsers__0__Password` - the seeded admin account; prefer these over putting a password in `appsettings.json`
- `APP_DB_PATH` - SQLite database location (default: /app/db/)
- `BUILD_VERSION` - Version number (YYYY.MM.DD format)
- `BUILD_SUFFIX` - Build identifier (GitHub run ID)

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `docker/proxysql.cnf` - ProxySQL server configuration
- `docker/docker-compose.yaml` - Local development stack
- `.dockerignore` - keeps `bin/`, `obj/`, `db/`, tests and **`appsettings.Development.json`** out of the image build context (the csproj also excludes that file from publish, so local secrets never ship)
- `global.json` - pins the .NET 10 SDK (`rollForward: latestFeature`) and selects the Microsoft.Testing.Platform test runner

## Testing

### Test Project
**Location:** `ProxysqlAdminUi.Tests/` (part of `ProxysqlAdminUi.sln`)
- **Framework:** xunit.v3 on Microsoft.Testing.Platform (test project is an `Exe`; `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`)
- **Type:** In-process integration tests via `WebApplicationFactory<Program>` (no Aspire, no live ProxySQL needed)
- **Run Tests:** `dotnet test` (or `dotnet test --project ProxysqlAdminUi.Tests/ProxysqlAdminUi.Tests.csproj`)

### Current Tests
- `Fixtures/ProxysqlAdminUiWebApplicationFactory.cs` - Boots the app with `APP_DB_PATH` pointed at a per-run temp folder (set in the constructor because `SqliteDbExtensions.GetAppDbPath()` reads the env var during service registration) and cleans it up on dispose
- `WebApplicationSmokeTests.cs` - Unauthenticated `/` redirects to `/Account/Login`, login page returns 200, identity DB is created, unknown routes 404

### Adding Tests
```csharp
public class MyTests(ProxysqlAdminUiWebApplicationFactory factory)
    : IClassFixture<ProxysqlAdminUiWebApplicationFactory>
{
    [Fact]
    public async Task LoginPage_ReturnsOk()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Account/Login", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```
Pages that load ProxySQL data render interactively (no prerendering), so HTTP-level tests only cover routing/auth; exercise `ProxySqlRepository` directly against a real ProxySQL for data-path checks.

## Important Conventions

### Naming Conventions
- **Models:** `{Entity}Model.cs` (e.g., `MysqlServerModel.cs`)
- **Pages:** `{Feature}Page.razor` or `{Entity}.razor`
- **ViewModels:** `{Feature}ViewModel.cs`
- **Services:** `{Purpose}Service.cs`

### Code Style
- C# file-scoped namespaces
- Dependency injection via constructor
- Async/await for database operations
- Razor component code-behind using `@code` blocks
- Identity form pages: `[SupplyParameterFromForm] private InputModel Input { get; set; } = default!;` plus `Input ??= new();` in `OnInitialized` (avoids analyzer BL0008)
- Never assign a Radzen component parameter from code (BL0005); bind it to a field, e.g. `Visible="@IsColumnVisible(...)"` in `QueryDigestPage.razor`

### Database Operations
- Use `ProxySqlRepository` methods (don't bypass the repository)
- Always use async methods (`async Task<T>`)
- Dispose DbContext properly (handled by DI)
- ProxySQL tables may require `LOAD ... TO RUNTIME` and `SAVE ... TO DISK`

## ProxySQL-Specific Notes

### Administrative Actions
After modifying ProxySQL configuration (servers, users, rules), execute:
1. `LOAD {TABLE} TO RUNTIME` - Apply changes
2. `SAVE {TABLE} TO DISK` - Persist changes

Example tables: `MYSQL SERVERS`, `MYSQL USERS`, `MYSQL QUERY RULES`, `MYSQL VARIABLES`

### Query Rules
- **Rule Matching:** Evaluated in order by `rule_id`
- **Digest:** MD5 hash of normalized query
- **Flagout:** Matching rule with flagOUT stops further evaluation
- **Cache TTL:** Set via `cache_ttl` column (milliseconds)

### Connection Flow
```
Client → ProxySQL (port 6033) → Backend MySQL Servers
         ↓
    Query Rules (routing/caching)
         ↓
    Hostgroups → Servers
```

## Troubleshooting

### Common Issues

**Issue:** Cannot connect to ProxySQL
- Check connection string in `appsettings.json`
- Verify ProxySQL is running on port 6033
- Ensure credentials (radmin/radmin) are correct

**Issue:** SQLite database locked
- Check APP_DB_PATH permissions
- Ensure directory `/app/db/` exists and is writable
- In Docker, verify volume mount

**Issue:** Changes not reflected in ProxySQL
- Execute `LOAD ... TO RUNTIME` after configuration changes
- Use Actions page or direct SQL execution

**Issue:** Authentication not working / default admin missing
- Check SQLite database exists and migrations applied
- Look for `Error seeding default users` in the startup log and fix whatever exception it reports; the seed service never throws, so a missing admin always shows up there first
- Review `DefaultUserSeedService.cs` logs

**Issue:** `dotnet test` reports "Zero tests ran"
- On the .NET 10 SDK the test project runs under Microsoft.Testing.Platform; pass the project with `--project` (or run `dotnet test` with no arguments), not as a positional argument

**Issue:** Build warning NU1510 / vulnerable transitive NuGet.* packages
- Do not re-add `Microsoft.AspNetCore.Components.Authorization` (implicit in the framework) or `Microsoft.VisualStudio.Web.CodeGeneration.Design` (design-time only; it ships Roslyn/NuGet into the publish output)

## Dependencies

### Critical NuGet Packages
- `Microsoft.AspNetCore.Components.Web` - Blazor components
- `Radzen.Blazor` - UI component library
- `MySql.EntityFrameworkCore` - MySQL data provider
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite data provider
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Identity system

### Dependabot Configuration
- **File:** `.github/dependabot.yml`
- **Schedule:** Weekly updates
- **Targets:** NuGet packages in `ProxysqlAdminUi.Web` and `ProxysqlAdminUi.Tests` (Microsoft.* and xunit* grouped), Docker base images in `docker/`, GitHub Actions

## Resources

### Official Documentation
- ProxySQL: https://proxysql.com/documentation/
- .NET 8: https://learn.microsoft.com/en-us/dotnet/
- Blazor: https://learn.microsoft.com/en-us/aspnet/core/blazor
- Radzen: https://blazor.radzen.com/

### Repository Structure
- Main branch: Production releases (Docker Hub)
- Dev branch: Development builds (private registry)
- Feature branches: Use standard GitHub flow

## Version Information

**Current Stack:**
- .NET 10.0 (LTS)
- Entity Framework Core 10.0.x
- MySql.EntityFrameworkCore 10.0.x (Oracle)
- Radzen Blazor 11.x
- xunit.v3 4.x
- ProxySQL 2.7.1 (via Docker)
- MariaDB 11 (via Docker)

**Versioning Scheme:**
- Format: `YYYY.MM.DD-{RUN_ID}`
- Example: `2025.11.04-123456`
- Display: Footer badge with GitHub release link

## Quick Reference

### Important Commands
```bash
# Development
dotnet watch --project ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj

# Build
dotnet build

# Test (no positional project path - see Testing section)
dotnet test

# Docker Compose
cd docker && docker-compose up -d

# Docker Build
docker build -f docker/Dockerfile -t proxysql-admin-ui .

# Migrations (Identity) - keep Up/Down empty unless you also drop the EnsureCreated pattern
dotnet ef migrations add MigrationName --context ProxysqlAdminUiWebAuthContext --project ProxysqlAdminUi.Web --startup-project ProxysqlAdminUi.Web --output-dir Migrations/AuthMigrations
```

### Key URLs (Local Development)
- Application: http://localhost:5203 (dotnet watch) or http://localhost:8000 (Docker)
- ProxySQL Web UI: http://localhost:6080
- ProxySQL MySQL: localhost:6033
- MariaDB: localhost:3306
- Swagger: removed (no API controllers) - `/swagger` is a 404 in every environment

---

**Last Updated:** 2026-09-12 (.NET 10 / Radzen 11 upgrade)
**Maintained By:** AI-assisted documentation generation

This document is intended for AI assistants (like Claude Code) to understand the codebase structure, conventions, and development workflows when assisting with development tasks.
