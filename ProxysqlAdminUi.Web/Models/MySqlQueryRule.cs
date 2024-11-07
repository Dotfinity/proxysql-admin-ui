using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.Web.Models;

[Table("mysql_query_rules")]
public class MysqlQueryRule
{
    [Key]
    [Column("rule_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RuleId { get; set; }

    [Column("active")]
    public bool Active { get; set; } = false;

    [Column("username")]
    [MaxLength(255)]
    public string? Username { get; set; }

    [Column("schemaname")]
    [MaxLength(255)]
    public string? Schemaname { get; set; }

    [Column("flagIN")]
    [Range(0, int.MaxValue)]
    public int FlagIn { get; set; } = 0;

    [Column("client_addr")]
    [MaxLength(255)]
    public string? ClientAddr { get; set; }

    [Column("proxy_addr")]
    [MaxLength(255)]
    public string? ProxyAddr { get; set; }

    [Column("proxy_port")]
    public int? ProxyPort { get; set; }

    [Column("digest")]
    [MaxLength(255)]
    public string? Digest { get; set; }

    [Column("match_digest")]
    [MaxLength(255)]
    public string? MatchDigest { get; set; }

    [Column("match_pattern")]
    [MaxLength(255)]
    public string? MatchPattern { get; set; }

    [Column("negate_match_pattern")]
    public bool NegateMatchPattern { get; set; } = false;

    [Column("re_modifiers")]
    [MaxLength(255)]
    public string? ReModifiers { get; set; } = "CASELESS";

    [Column("flagOUT")]
    [Range(0, int.MaxValue)]
    public int? FlagOut { get; set; }

    [Column("replace_pattern")]
    [MaxLength(255)]
    public string? ReplacePattern { get; set; }

    [Column("destination_hostgroup")]
    public int? DestinationHostgroup { get; set; }

    [Column("cache_ttl")]
    [Range(1, int.MaxValue)]
    public int? CacheTtl { get; set; }

    [Column("cache_empty_result")]

    public bool? CacheEmptyResult { get; set; }

    [Column("cache_timeout")]
    [Range(0, int.MaxValue)]
    public int? CacheTimeout { get; set; }

    [Column("reconnect")]
    public bool? Reconnect { get; set; }

    [Column("timeout")]
    public uint? Timeout { get; set; }

    [Column("retries")]
    [Range(0, 1000)]
    public int? Retries { get; set; }

    [Column("delay")]
    public uint? Delay { get; set; }

    [Column("next_query_flagIN")]
    public uint? NextQueryFlagIn { get; set; }

    [Column("mirror_flagOUT")]
    public uint? MirrorFlagOut { get; set; }

    [Column("mirror_hostgroup")]
    public uint? MirrorHostgroup { get; set; }

    [Column("error_msg")]
    [MaxLength(255)]
    public string? ErrorMsg { get; set; }

    [Column("OK_msg")]
    [MaxLength(255)]
    public string? OKMsg { get; set; }

    [Column("sticky_conn")]

    public bool? StickyConn { get; set; }

    [Column("multiplex")]
    [Range(0, 2)]
    public int? Multiplex { get; set; }

    [Column("gtid_from_hostgroup")]
    public uint? GtidFromHostgroup { get; set; }

    [Column("log")]
    public bool? Log { get; set; }

    [Column("apply")]

    public bool Apply { get; set; } = false;

    [Column("attributes")]
    [Required]
    [MaxLength(255)]
    public string? Attributes { get; set; } = string.Empty;

    [Column("comment")]
    [MaxLength(255)]
    public string? Comment { get; set; }
}
