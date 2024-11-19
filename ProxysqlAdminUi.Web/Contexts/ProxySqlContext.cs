using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.Contexts;

public class ProxySqlContext(DbContextOptions<ProxySqlContext> options) : DbContext(options)
{
    public DbSet<MysqlQueryRuleModel> MySqlQueryRules { get; set; }
    public DbSet<StatsMySqlQueryRuleModel> StatsMySqlQueryRules { get; set; }
    public DbSet<StatsMysqlQueryDigestModel> StatsMySqlQueryDigests { get; set; }
    public DbSet<MysqlServerModel> MySqlServers { get; set; }
    public DbSet<MysqlUserModel> MySqlUsers { get; set; }

    public DbSet<GlobalVariableModel> GlobalVariables { get; set; }

    public DbSet<StatsMySqlGlobalModel> StatsMySqlGlobals { get; set; }
    public DbSet<StatsMemoryMetricsModel> StatsMemoryMetrics { get; set; }
}
