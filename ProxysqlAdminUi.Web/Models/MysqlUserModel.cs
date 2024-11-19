using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.Web.Models;

[Table("mysql_users")]
[PrimaryKey(nameof(Username), nameof(Backend))]
public class MysqlUserModel
{

    [Column("username", Order = 0)]
    [Required]
    [MaxLength(255)]
    public string Username { get; set; }

    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; }

    [Column("active")]
    [Range(0, 1)]
    public int Active { get; set; } = 1;

    [Column("use_ssl")]
    [Range(0, 1)]
    public int UseSsl { get; set; } = 0;

    [Column("default_hostgroup")]
    public int DefaultHostgroup { get; set; } = 0;

    [Column("default_schema")]
    [MaxLength(255)]
    public string DefaultSchema { get; set; }

    [Column("schema_locked")]
    [Range(0, 1)]
    public int SchemaLocked { get; set; } = 0;

    [Column("transaction_persistent")]
    [Range(0, 1)]
    public int TransactionPersistent { get; set; } = 1;

    [Column("fast_forward")]
    [Range(0, 1)]
    public int FastForward { get; set; } = 0;


    [Column("backend", Order = 1)]
    [Range(0, 1)]
    public int Backend { get; set; } = 1;

    [Column("frontend")]
    [Range(0, 1)]
    public int Frontend { get; set; } = 1;

    [Column("max_connections")]
    [Range(0, int.MaxValue)]
    public int MaxConnections { get; set; } = 10000;

    [Column("attributes")]
    [Required]
    [MaxLength(255)]
    public string Attributes { get; set; } = string.Empty;

    [Column("comment")]
    [Required]
    [MaxLength(255)]
    public string Comment { get; set; } = string.Empty;
}
