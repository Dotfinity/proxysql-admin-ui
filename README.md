# Proxysql-admin-ui

## Todo

- [ ] Add a way to add new servers
- [ ] Add a way to add new users
- [ ] Fix login page layout
- [ ] Add logout button
- [ ] Add support for environment variables for configuration
- [ ] Custom db folder for sqlite
- [ ] Use the `hostgroup -1` to show the stats of the cached queries, as that's not reset on LOAD STATS;
-------

## Done

- [x] Add a gauge to show the total number of cached vs uncached queries
- [x] On the query digest page, add a button to cache the query for 1 minute, 5 minutes, 1 hour, 1 day, the button should be dropdown
