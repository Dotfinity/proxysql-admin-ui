using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace ProxysqlAdminUi.Web.Models;

[Table("mysql_servers")]
[PrimaryKey(nameof(HostgroupId), nameof(Hostname), nameof(Port))]
public class MysqlServerModel
{

    [Column("hostgroup_id", Order = 0)]
    [Range(0, int.MaxValue)]
    public int HostgroupId { get; set; }


    [Column("hostname", Order = 1)]
    [Required]
    [MaxLength(255)]
    public string Hostname { get; set; }


    [Column("port", Order = 2)]
    [Range(0, 65535)]
    public int Port { get; set; } = 3306;

    [Column("gtid_port")]
    [Range(0, 65535)]
    public int GtidPort { get; set; } = 0;

    [Column("status")]
    [Required]
    [MaxLength(50)]
    [RegularExpression("ONLINE|SHUNNED|OFFLINE_SOFT|OFFLINE_HARD", ErrorMessage = "Invalid status")]
    public string Status { get; set; } = "ONLINE";

    [Column("weight")]
    [Range(0, 10000000)]
    public int Weight { get; set; } = 1;

    [Column("compression")]
    [Range(0, 1)]
    public int Compression { get; set; } = 0;

    [Column("max_connections")]
    [Range(0, int.MaxValue)]
    public int MaxConnections { get; set; } = 1000;

    [Column("max_replication_lag")]
    [Range(0, 126144000)]
    public int MaxReplicationLag { get; set; } = 0;

    [Column("use_ssl")]
    [Range(0, 1)]
    public int UseSsl { get; set; } = 0;

    [Column("max_latency_ms")]
    [Range(0, uint.MaxValue)]
    public uint MaxLatencyMs { get; set; } = 0;

    [Column("comment")]
    [Required]
    [MaxLength(255)]
    public string Comment { get; set; } = string.Empty;
}

