using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.Web.Models;

[Table("stats_mysql_query_rules")]
public class StatsMySqlQueryRule
{
    [Key]
    [Column("rule_id")]
    public int RuleId { get; set; }

    [Column("hits")]
    [Required]
    public int Hits { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (StatsMySqlQueryRule)obj;
        return RuleId == other.RuleId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RuleId);
    }
}