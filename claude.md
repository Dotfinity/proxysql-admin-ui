# ProxySQL Admin UI - AI Assistant Context

## Project Overview

**ProxySQL Admin UI** is a modern web-based administration dashboard for managing ProxySQL (a high-performance MySQL proxy server). Built with .NET 8 and Blazor Server, it provides a user-friendly interface for monitoring performance, configuring backend servers, managing user credentials, and defining query routing rules.

**License:** MIT
**Primary Language:** C# (.NET 8)
**Web Framework:** Blazor Server with Radzen UI Components

## Quick Facts

- **Main Project:** `ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj`
- **Startup File:** `ProxysqlAdminUi.Web/Program.cs`
- **Entry Point:** ASP.NET Core web server on port 8000 (configurable)
- **Deployment:** Docker containers (Alpine Linux based)
- **Databases:**
  - ProxySQL (MySQL protocol on port 6033)
  - SQLite (local identity/authentication)

## Technology Stack

### Core Framework
- **.NET 8** - LTS version with Alpine Linux runtime
- **ASP.NET Core** - Web framework
- **Blazor Server** - Interactive server-side rendering
- **Entity Framework Core 8.0.10** - ORM for data access

### UI & Frontend
- **Radzen Blazor Components** - Material Design component library
- **Material Design Icons** - Icon set
- **Interactive Server Rendering** - Real-time UI updates via SignalR

### Data Access
- **MySql.EntityFrameworkCore** - ProxySQL connection via MySQL protocol
- **Microsoft.EntityFrameworkCore.Sqlite** - Local authentication database
- **Direct SQL Execution** - For complex ProxySQL-specific operations

### Supporting Libraries
- **Newtonsoft.Json 13.0.3** - JSON serialization
- **Swashbuckle.AspNetCore 6.9.0** - OpenAPI/Swagger documentation
- **ASP.NET Core Identity** - Cookie-based authentication

### Testing
- **xUnit 2.9.0** - Unit testing framework
- **Aspire.Hosting.Testing** - Integration testing
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
│   ├── Account/              # Authentication UI (Login, Register, etc.)
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
- **`Program.cs`** (108 lines) - Core application setup:
  - Registers services (Blazor, Radzen, Identity, DbContexts, Repository)
  - Configures authentication (cookie-based)
  - Database initialization and migrations
  - Swagger/OpenAPI setup

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
- **Port:** 6033 (ProxySQL MySQL interface)
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
**Location:** `/app/db/app.db` (Docker) or `./db/app.db` (local)
- **Provider:** SQLite via Entity Framework Core
- **Purpose:** ASP.NET Core Identity (user authentication)
- **Tables:** AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
- **Migrations:** Located in `Migrations/AuthMigrations/`

## Development Workflow

### Local Development
```bash
# Run with hot reload
dotnet watch --project ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj

# Build
dotnet build

# Run tests
dotnet test

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
- **Account Pages:** `Components/Account/` (Login, Register, etc.)
- **Configuration:** `Program.cs` - Identity services registration

## Build & Deployment

### Docker Build
**Dockerfile:** `docker/Dockerfile` (multi-stage build)
- **Stage 1:** Build with .NET SDK 8.0 Alpine
- **Stage 2:** Runtime with ASP.NET Core 8.0 Alpine
- **Output:** Optimized Alpine-based image (~350MB)

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
- `APP_DB_PATH` - SQLite database location (default: /app/db/)
- `BUILD_VERSION` - Version number (YYYY.MM.DD format)
- `BUILD_SUFFIX` - Build identifier (GitHub run ID)

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `docker/proxysql.cnf` - ProxySQL server configuration
- `docker/docker-compose.yaml` - Local development stack

## Testing

### Test Project
**Location:** `ProxysqlAdminUi.Tests/`
- **Framework:** xUnit
- **Type:** Integration tests using Aspire.Hosting.Testing
- **Run Tests:** `dotnet test`

### Current Tests
- `WebTests.cs` - Verifies application starts and returns HTTP 200

### Adding Tests
```csharp
[Fact]
public async Task TestName()
{
    // Arrange
    var appHost = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.ProxysqlAdminUi_Web>();

    // Act
    var resource = appHost.Resources.Single(r => r.Name == "webfrontend");

    // Assert
    // ...
}
```

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

**Issue:** Authentication not working
- Check SQLite database exists and migrations applied
- Verify default user creation in `Program.cs`
- Review `DefaultUserSeedService.cs` logs

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
- **Target:** NuGet packages in ProxysqlAdminUi.Web

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
- .NET 8.0 (LTS)
- Entity Framework Core 8.0.10
- Radzen Blazor (latest)
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

# Test
dotnet test

# Docker Compose
cd docker && docker-compose up -d

# Docker Build
docker build -f docker/Dockerfile -t proxysql-admin-ui .

# Migrations (Identity)
dotnet ef migrations add MigrationName --context ProxysqlAdminUiWebAuthContext --project ProxysqlAdminUi.Web
```

### Key URLs (Local Development)
- Application: http://localhost:5203 (dotnet watch) or http://localhost:8000 (Docker)
- ProxySQL Web UI: http://localhost:6080
- ProxySQL MySQL: localhost:6033
- MariaDB: localhost:3306
- Swagger: http://localhost:8000/swagger (if enabled)

---

**Last Updated:** 2025-11-04
**Maintained By:** AI-assisted documentation generation

This document is intended for AI assistants (like Claude Code) to understand the codebase structure, conventions, and development workflows when assisting with development tasks.
