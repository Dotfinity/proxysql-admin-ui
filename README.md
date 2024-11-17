# ProxySql admin ui

ProxySQL Admin UI is a simple web interface that allows you to manage ProxySQL without having to use the command line or the MySQL client.

## Features

- [x] Manage query cache rules
  - [x] View query cache rules
  - [x] Add new query cache rules
  - [x] Edit query cache rules
  - [x] Remove query cache rules
- [x] View query statistics
  - [x] Sorting, filtering, and pagination
- [x] View the status of the ProxySQL server
- [ ] Manage mysql servers
  - [x] View servers
  - [ ] Add new servers
  - [ ] Remove servers
  - [ ] Edit servers
- [ ] Manage users
  - [x] View users
  - [ ] Add new users
  - [ ] Remove users
  - [ ] Edit users


## Configuring the project

Set the environment variable `ASPNETCORE_URLS` to the urls you want to use, for example:
```
ASPNETCORE_URLS=http://*:80/;http://*:8080/;https://*:443/;https://*:8081/
```
or
```
http://*:5000;http://localhost:5001;https://hostname:5002
```

---
## Todo

- [ ] Add a way to add new servers
- [ ] Add a way to add new users
- [ ] Fix login page layout
- [ ] Add logout button
- [ ] Add support for environment variables for configuration
- [ ] Use the `hostgroup -1` to show the stats of the cached queries, as that's not reset on LOAD STATS;
- [ ] Add docker compose + caddy to run the project
- [ ] improve the home page layout
- [ ] the homepage cache gauge shows "wrong" data, it should show the number of cached queries vs the number of uncached queries, not the cache efficiency of query rules. The existing gauge should be renamed to "cache efficiency" and a new gauge should be added to show the number of cached queries vs the number of uncached queries
- [ ] rows should be highlighted when hovering over them on the query digest page

-------

## Done

- [x] Add a way to refresh query digest stats without resetting the selected filters
- [x] Custom db folder for sqlite
- [x] Add a gauge to show the total number of cached vs uncached queries
- [x] On the query digest page, add a button to cache the query for 1 minute, 5 minutes, 1 hour, 1 day, the button should be dropdown
