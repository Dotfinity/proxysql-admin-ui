using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Contexts;
using ProxysqlAdminUi.Web.Models;
using ProxysqlAdminUi.Web.ViewModel;

namespace ProxysqlAdminUi.Web.Repositories;

public class ProxySqlRepository(ProxySqlContext dbContext)
{
  // MySQL Servers
  public async Task<IEnumerable<MysqlServer>> GetMySqlServers()
  {
    return await dbContext.MySqlServers
        .FromSqlRaw("SELECT * FROM mysql_servers")
        .ToListAsync();
  }

  public async Task<MysqlServer> GetMySqlServer(string hostname)
  {
    return await dbContext.MySqlServers
        .FromSqlRaw("SELECT * FROM mysql_servers WHERE hostname = {0}", hostname)
        .FirstOrDefaultAsync();
  }

  public async Task<int> AddMySqlServer(MysqlServer server)
  {
    var sql = @"INSERT INTO mysql_servers 
            (hostgroup_id, hostname, port, gtid_port, status, weight, compression, 
             max_connections, max_replication_lag, use_ssl, max_latency_ms, comment)
            VALUES 
            ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})";

    var result = await dbContext.Database.ExecuteSqlRawAsync(sql,
         server.HostgroupId, server.Hostname, server.Port, server.GtidPort,
         server.Status, server.Weight, server.Compression, server.MaxConnections,
         server.MaxReplicationLag, server.UseSsl, server.MaxLatencyMs, server.Comment);

    return result;
  }

  public async Task<int> UpdateMySqlServer(MysqlServer server)
  {
    var sql = @"UPDATE mysql_servers 
            SET status = {2}, weight = {3}, compression = {4},
                max_connections = {5}, max_replication_lag = {6},
                use_ssl = {7}, max_latency_ms = {8}, comment = {9}
            WHERE hostgroup_id = {0} AND hostname = {1}";

    return await dbContext.Database.ExecuteSqlRawAsync(sql,
        server.HostgroupId, server.Hostname, server.Status, server.Weight,
        server.Compression, server.MaxConnections, server.MaxReplicationLag,
        server.UseSsl, server.MaxLatencyMs, server.Comment);
  }

  public async Task<int> DeleteMySqlServer(int hostgroupId, string hostname)
  {
    return await dbContext.Database.ExecuteSqlRawAsync(
        "DELETE FROM mysql_servers WHERE hostgroup_id = {0} AND hostname = {1}",
        hostgroupId, hostname);
  }

  // MySQL Users
  public async Task<IEnumerable<MysqlUser>> GetMySqlUsers()
  {
    return await dbContext.MySqlUsers
        .FromSqlRaw("SELECT * FROM mysql_users")
        .ToListAsync();
  }

  public async Task<MysqlUser> GetMySqlUser(string username)
  {
    return await dbContext.MySqlUsers
        .FromSqlRaw("SELECT * FROM mysql_users WHERE username = {0}", username)
        .FirstOrDefaultAsync();
  }

  public async Task<MysqlUser> AddMySqlUser(MysqlUser user)
  {
    var sql = @"INSERT INTO mysql_users 
            (username, password, active, use_ssl, default_hostgroup, default_schema,
             schema_locked, transaction_persistent, fast_forward, backend, frontend,
             max_connections, attributes, comment)
            VALUES 
            ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})";

    await dbContext.Database.ExecuteSqlRawAsync(sql,
        user.Username, user.Password, user.Active, user.UseSsl,
        user.DefaultHostgroup, user.DefaultSchema, user.SchemaLocked,
        user.TransactionPersistent, user.FastForward, user.Backend,
        user.Frontend, user.MaxConnections, user.Attributes, user.Comment);

    return user;
  }

  public async Task<int> UpdateMySqlUser(MysqlUser user)
  {
    var sql = @"UPDATE mysql_users 
            SET password = {1}, active = {2}, use_ssl = {3},
                default_hostgroup = {4}, default_schema = {5},
                schema_locked = {6}, transaction_persistent = {7},
                fast_forward = {8}, frontend = {9},
                max_connections = {10}, attributes = {11}, comment = {12}
            WHERE username = {0} AND backend = {13}";

    return await dbContext.Database.ExecuteSqlRawAsync(sql,
        user.Username, user.Password, user.Active, user.UseSsl,
        user.DefaultHostgroup, user.DefaultSchema, user.SchemaLocked,
        user.TransactionPersistent, user.FastForward, user.Frontend,
        user.MaxConnections, user.Attributes, user.Comment, user.Backend);
  }

  public async Task<int> DeleteMySqlUser(string username)
  {
    return await dbContext.Database.ExecuteSqlRawAsync(
        "DELETE FROM mysql_users WHERE username = {0}", username);
  }

  // MySQL Query Rules
  public async Task<IEnumerable<MysqlQueryRule>> GetMySqlQueryRules()
  {
    return await dbContext.MySqlQueryRules
        .FromSqlRaw("SELECT * FROM mysql_query_rules")
        .ToListAsync();
  }

  public async Task<IEnumerable<QueryRuleViewModel>> GetQueryRulesWithStats()
  {
    const string sql = @"
SELECT r.*, 
    COALESCE(s.hits, 0) as Hits, 
    MAX(d.digest_text) as DigestText,
    COALESCE(SUM(CASE WHEN d.hostgroup >= 0 THEN d.count_star ELSE 0 END), 0) as CountStar
FROM mysql_query_rules r 
LEFT JOIN stats_mysql_query_rules s ON r.rule_id = s.rule_id
LEFT JOIN stats_mysql_query_digest d ON r.digest = d.digest
GROUP BY r.rule_id, r.active, r.username, r.schemaname, r.flagIN, r.client_addr, 
         r.proxy_addr, r.proxy_port, r.digest, r.match_digest, r.match_pattern, 
         r.negate_match_pattern, r.re_modifiers, r.flagOUT, r.replace_pattern,
         r.destination_hostgroup, r.cache_ttl, r.cache_empty_result, r.cache_timeout,
         r.reconnect, r.timeout, r.retries, r.delay, r.next_query_flagIN, 
         r.mirror_flagOUT, r.mirror_hostgroup, r.error_msg, r.OK_msg, r.sticky_conn,
         r.multiplex, r.gtid_from_hostgroup, r.log, r.apply, r.attributes, r.comment,
         s.hits";

    return dbContext.Database.SqlQueryRaw<QueryRuleViewModel>(sql);
  }

  public async Task<MysqlQueryRule> GetMySqlQueryRule(int ruleId)
  {
    return await dbContext.MySqlQueryRules
        .FromSqlRaw("SELECT * FROM mysql_query_rules WHERE rule_id = {0}", ruleId)
        .FirstOrDefaultAsync();
  }

  public async Task<int> AddMySqlQueryRule(MysqlQueryRule rule)
  {
    var sql = @"INSERT INTO mysql_query_rules 
            (active, username, schemaname, flagIN, client_addr, proxy_addr,
             proxy_port, digest, match_digest, match_pattern, negate_match_pattern,
             re_modifiers, flagOUT, replace_pattern, destination_hostgroup,
             cache_ttl, cache_empty_result, cache_timeout, reconnect, timeout,
             retries, delay, next_query_flagIN, mirror_flagOUT, mirror_hostgroup,
             error_msg, OK_msg, sticky_conn, multiplex, gtid_from_hostgroup,
             log, apply, attributes, comment)
            VALUES 
            ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11},
             {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21},
             {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31},
             {32}, {33})";

    var result = await dbContext.Database.ExecuteSqlRawAsync(sql,
        rule.Active, rule.Username, rule.Schemaname, rule.FlagIn,
        rule.ClientAddr, rule.ProxyAddr, rule.ProxyPort, rule.Digest,
        rule.MatchDigest, rule.MatchPattern, rule.NegateMatchPattern,
        rule.ReModifiers, rule.FlagOut, rule.ReplacePattern,
        rule.DestinationHostgroup, rule.CacheTtl, rule.CacheEmptyResult,
        rule.CacheTimeout, rule.Reconnect, rule.Timeout, rule.Retries,
        rule.Delay, rule.NextQueryFlagIn, rule.MirrorFlagOut,
        rule.MirrorHostgroup, rule.ErrorMsg, rule.OKMsg, rule.StickyConn,
        rule.Multiplex, rule.GtidFromHostgroup, rule.Log, rule.Apply,
        rule.Attributes, rule.Comment);

    await LoadRulesToRuntime();
    await SaveRulesToDisk();
    return result;
  }

  public async Task<int> UpdateMySqlQueryRule(MysqlQueryRule rule)
  {
    var sql = @"UPDATE mysql_query_rules 
            SET active = {1}, username = {2}, schemaname = {3},
                flagIN = {4}, client_addr = {5}, proxy_addr = {6},
                proxy_port = {7}, digest = {8}, match_digest = {9},
                match_pattern = {10}, negate_match_pattern = {11},
                re_modifiers = {12}, flagOUT = {13}, replace_pattern = {14},
                destination_hostgroup = {15}, cache_ttl = {16},
                cache_empty_result = {17}, cache_timeout = {18},
                reconnect = {19}, timeout = {20}, retries = {21},
                delay = {22}, next_query_flagIN = {23}, mirror_flagOUT = {24},
                mirror_hostgroup = {25}, error_msg = {26}, OK_msg = {27},
                sticky_conn = {28}, multiplex = {29}, gtid_from_hostgroup = {30},
                log = {31}, apply = {32}, attributes = {33}, comment = {34}
            WHERE rule_id = {0}";

    var result = await dbContext.Database.ExecuteSqlRawAsync(sql,
        rule.RuleId, rule.Active, rule.Username, rule.Schemaname,
        rule.FlagIn, rule.ClientAddr, rule.ProxyAddr, rule.ProxyPort,
        rule.Digest, rule.MatchDigest, rule.MatchPattern,
        rule.NegateMatchPattern, rule.ReModifiers, rule.FlagOut,
        rule.ReplacePattern, rule.DestinationHostgroup, rule.CacheTtl,
        rule.CacheEmptyResult, rule.CacheTimeout, rule.Reconnect,
        rule.Timeout, rule.Retries, rule.Delay, rule.NextQueryFlagIn,
        rule.MirrorFlagOut, rule.MirrorHostgroup, rule.ErrorMsg,
        rule.OKMsg, rule.StickyConn, rule.Multiplex,
        rule.GtidFromHostgroup, rule.Log, rule.Apply,
        rule.Attributes, rule.Comment);

    await LoadRulesToRuntime();
    await SaveRulesToDisk();
    return result;
  }

  public async Task<int> DeleteMySqlQueryRule(int ruleId)
  {
    var result = await dbContext.Database.ExecuteSqlRawAsync(
        "DELETE FROM mysql_query_rules WHERE rule_id = {0}", ruleId);
    await LoadRulesToRuntime();
    await SaveRulesToDisk();
    return result;
  }

  // Stats
  public async Task<IEnumerable<QueryDigestViewModel>> GetStatsMySqlQueryDigests()
  {
    const string sql = @"
        SELECT d.*,
               CASE WHEN r.rule_id IS NOT NULL THEN 1 ELSE 0 END as HasRule,
               r.rule_id as RuleId
        FROM stats_mysql_query_digest d
        LEFT JOIN mysql_query_rules r ON d.digest = r.digest
        where d.hostgroup >=0";

    return await dbContext.Database.SqlQueryRaw<QueryDigestViewModel>(sql)
        .ToListAsync();
  }

  public async Task<IEnumerable<StatsMySqlQueryRule>> GetStatsMySqlQueryRules()
  {
    return await dbContext.StatsMySqlQueryRules
        .FromSqlRaw("SELECT * FROM stats_mysql_query_rules")
        .ToListAsync();
  }

  public async Task LoadRulesToRuntime()
  {
    await dbContext.Database.ExecuteSqlRawAsync("LOAD MYSQL QUERY RULES TO RUNTIME;");
  }

  public async Task SaveRulesToDisk()
  {
    await dbContext.Database.ExecuteSqlRawAsync("SAVE MYSQL QUERY RULES TO DISK;");
  }


  public async Task<IEnumerable<GlobalVariableModel>> GetGlobalVariables()
  {
      return await dbContext.GlobalVariables.ToListAsync();
  }

  public async Task<int> UpdateGlobalVariable(GlobalVariableModel variable)
  {
      var result = await dbContext.Database.ExecuteSqlRawAsync(
          "UPDATE global_variables SET variable_value = {0} WHERE variable_name = {1}",
          variable.VariableValue, variable.VariableName);

      await dbContext.Database.ExecuteSqlRawAsync(
          "LOAD ADMIN VARIABLES TO RUNTIME");

      return result;
  }
}