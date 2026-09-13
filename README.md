![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/dotfinity/proxysql-admin-ui/docker-container-publish.yml)

----

# ProxySQL Admin UI

A modern web interface for managing ProxySQL, built with Blazor and .NET Core.

## Screenshots

<a href="https://img.dotfinity.eu/proxysql-admin-ui-dashboard-page.png">
  <img src="https://img.dotfinity.eu/proxysql-admin-ui-dashboard-page.png" alt="Dashboard" width="400" height="200"/>
</a>
<a href="https://img.dotfinity.eu/proxysql-admin-ui-query-digest-stats-page.png">
  <img src="https://img.dotfinity.eu/proxysql-admin-ui-query-digest-stats-page.png" alt="Qiery digest stats" width="400" height="200"/>
</a>
<a href="https://img.dotfinity.eu/proxysql-admin-ui-query-rules-page.png">
  <img src="https://img.dotfinity.eu/proxysql-admin-ui-query-rules-page.png" alt="Query rules page" width="400" height="200"/>
</a>
<a href="https://img.dotfinity.eu/proxysql-admin-ui-global-variables-page.png">
  <img src="https://img.dotfinity.eu/proxysql-admin-ui-global-variables-page.png" alt="Admin global variables editor page" width="400" height="200"/>
</a>

## Features

- Dashboard with metrics
- ProxySQL management:
  - Mysql backend server configuration
  - Mysql Users configuration
  - Query rules configuration
  - Query digest grid with stats and filters
  - Global variables configuration

## Tech Stack

- ASP.NET Core (.NET 10 LTS)
- Blazor Server
- Entity Framework Core 10 with the Oracle `MySql.EntityFrameworkCore` provider
- [Radzen Blazor Components](https://blazor.radzen.com/) 11
- SQLite (for auth data)
- xunit.v3 smoke tests (`dotnet test`)

## Getting Started

### Prerequisites

- .NET 10 SDK
- Running ProxySQL instance (the app talks to the **admin** interface, port 6032 by default in ProxySQL)

### Configuration

#### Local Configuration

1. Configure ProxySQL connection in `appsettings.json`:

```json
"ConnectionStrings": {
    "ProxySqlContext": "Server=0.0.0.0;Port=6032;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;"
  },
  "DefaultUsers": [
    {
      "Username": "admin",
      "Password": "AdminAdminAdmin1"
    }
  ]
```

The `DefaultUsers` entry seeds the first account. This checked-in password is a placeholder: the app
forces a password change on the first request while it is still in use. For anything but a local
sandbox, set it through `PAI_DefaultUsers__0__Password` instead of editing this file - see
[Docker Configuration](#docker-configuration).

2. Start the application:

```bash
dotnet watch --project ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj
```

3. Run the tests:

```bash
dotnet test
```

#### Docker Configuration

Set the environment variables in the `docker-compose.yml` file:

```yml
environment:
  ASPNETCORE_ENVIRONMENT: Production
  ASPNETCORE_URLS: 'http://+:8000'
  # Connection string for the ProxySQL server
  PAI_ConnectionStrings__ProxySqlContext: 'Server=xxxxxx;Port=6032;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;'
```

or if you're running the app and the proxysql server in the same docker-compose file, you can use the service name as the host:

```yml
environment:
  ASPNETCORE_ENVIRONMENT: Production
  ASPNETCORE_URLS: 'http://+:8000'
  # Connection string for the ProxySQL server
  PAI_ConnectionStrings__ProxySqlContext: 'Server=proxysql;Port=6032;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;'
```

#### The initial administrator account

Supply the seeded account as environment variables rather than editing `appsettings.json`, so a real
password never ends up in the image or in the compose file:

```yml
environment:
  PAI_DefaultUsers__0__Username: 'admin'
  PAI_DefaultUsers__0__Password: '${PAI_ADMIN_PASSWORD}'   # from an .env file or a secret store
```

The account is created on first start, when the identity database is empty. As long as it still
authenticates with the configured password, the app redirects every request to
**Account → Change password** and refuses to go anywhere else — a published default credential is
never left standing. Once the password is changed, normal navigation resumes.

There is no self-registration and no password-reset e-mail: accounts are created only from
`DefaultUsers`.

#### Recovering a lost admin password

Because there is no password-reset flow, recovery happens at container level. Stop the app, delete
the identity database - `db/app.db` locally, or the file inside the volume mounted at `APP_DB_PATH`
(`/app/db/` by default) - and start it again. The seed service recreates the schema and the
`DefaultUsers` account, and the forced password change applies to it as before.

Note that this also discards any other accounts and their two-factor registrations. If the account is
merely locked out after five failed sign-ins, nothing needs deleting: the lock expires after five
minutes on its own.

## Random notes

>One query rule can have many digests! 
> example: if one rule matches by table prefix "ps_setting*"
> if there are 10 tables being accessed, via 20 queries, that would result in 40 different digests. 
> 20 for the uncached queries and 20 for the cache hits. 


## License

[MIT](/LICENSE.md)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
