using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.Contexts;

public class ProxySqlContext : DbContext
{
    public ProxySqlContext(DbContextOptions<ProxySqlContext> options) : base(options)
    {
    }

    public DbSet<MysqlQueryRule> MySqlQueryRules { get; set; }
    public DbSet<StatsMySqlQueryRule> StatsMySqlQueryRules { get; set; }
    public DbSet<StatsMysqlQueryDigest> StatsMySqlQueryDigests { get; set; }

    public DbSet<MysqlServer> MySqlServers { get; set; }
    public DbSet<MysqlUser> MySqlUsers { get; set; }


}
