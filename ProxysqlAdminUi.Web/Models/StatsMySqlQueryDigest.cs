using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.Web.Models;

[Table("stats_mysql_query_digest")]
[PrimaryKey(nameof(Hostgroup), nameof(Schemaname), nameof(Username), nameof(ClientAddress), nameof(Digest))]
public class StatsMysqlQueryDigest
{

    [Column("hostgroup", Order = 0)]
    public int Hostgroup { get; set; }


    [Column("schemaname", Order = 1)]
    [Required]
    [MaxLength(255)]
    public string Schemaname { get; set; }


    [Column("username", Order = 2)]
    [Required]
    [MaxLength(255)]
    public string Username { get; set; }


    [Column("client_address", Order = 3)]
    [Required]
    [MaxLength(255)]
    public string ClientAddress { get; set; }


    [Column("digest", Order = 4)]
    [Required]
    [MaxLength(255)]
    public string Digest { get; set; }

    [Column("digest_text")]
    [Required]
    [MaxLength(255)]
    public string DigestText { get; set; }

    [Column("count_star")]
    [Required]
    public long CountStar { get; set; }

    [Column("first_seen")]
    [Required]
    public int FirstSeen { get; set; }

    [Column("last_seen")]
    [Required]
    public int LastSeen { get; set; }

    [Column("sum_time")]
    [Required]
    public long SumTime { get; set; }

    [Column("min_time")]
    [Required]
    public int MinTime { get; set; }

    [Column("max_time")]
    [Required]
    public int MaxTime { get; set; }

    [Column("sum_rows_affected")]
    [Required]
    public long SumRowsAffected { get; set; }

    [Column("sum_rows_sent")]
    [Required]
    public long SumRowsSent { get; set; }
}