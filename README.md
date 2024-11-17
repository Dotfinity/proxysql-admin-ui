# ProxySQL Admin UI

A modern web interface for managing ProxySQL, built with Blazor and .NET Core.

## Features

- Dashboard with metrics
- ProxySQL management:
  - Mysql backend server configuration
  - Mysql Users configuration
  - Query rules configuration
  - Query digest grid with stats and filters
  - Global variables configuration

## Tech Stack

- ASP.NET Core (.NET 8)  
- Blazor Server
- Entity Framework Core
- [Radzen Blazor Components](https://blazor.radzen.com/)
- SQLite (for auth data)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Running ProxySQL instance

### Configuration

#### Local Configuration

1. Configure ProxySQL connection in `appsettings.json`:

```json
"ConnectionStrings": {
    "ProxySqlContext": "Server=0.0.0.0;Port=6033;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;"
  },
  "DefaultUsers": [
    {
      "Username": "admin",
      "Password": "AdminAdminAdmin1"
    }
  ]
```

2. Start the application:

```bash
dotnet watch --project ProxysqlAdminUi.Web/ProxysqlAdminUi.Web.csproj
```

#### Docker Configuration

Set the environment variables in the `docker-compose.yml` file:

```yml
environment:
  ASPNETCORE_ENVIRONMENT: Production
  ASPNETCORE_URLS: 'http://+:8000'
  # Connection string for the ProxySQL server
  PAI_ConnectionStrings__ProxySqlContext: 'Server=xxxxxx;Port=6033;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;'
```

or if you're running the app and the proxysql server in the same docker-compose file, you can use the service name as the host:

```yml
environment:
  ASPNETCORE_ENVIRONMENT: Production
  ASPNETCORE_URLS: 'http://+:8000'
  # Connection string for the ProxySQL server
  PAI_ConnectionStrings__ProxySqlContext: 'Server=proxysql;Port=6033;Uid=radmin;Pwd=radmin;ConnectionReset=False;Pooling=True;ConnectionLifeTime=3000000;'
```


## License

[MIT](/LICENSE.md)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
