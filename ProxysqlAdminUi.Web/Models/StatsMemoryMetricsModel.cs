using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.Web.Models;

[Table("stats_memory_metrics")]
public class StatsMemoryMetricsModel
{
    [Key]
    [Column("Variable_Name")]
    public string VariableName { get; set; }

    [Column("Variable_Value")]
    public long VariableValue { get; set; }
}