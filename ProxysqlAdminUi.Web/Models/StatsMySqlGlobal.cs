using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.Web.Models;

[Table("stats_mysql_global")]
public class StatsMySqlGlobal
{
    [Key]
    [Column("Variable_Name")]
    public string VariableName { get; set; }

    [Column("Variable_Value")]
    public string VariableValue { get; set; }
}