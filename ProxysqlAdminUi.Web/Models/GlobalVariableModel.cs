using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxysqlAdminUi.Web.Models;

[Table("global_variables")]
public class GlobalVariableModel
{
    [Key]
    [Column("variable_name")]
    [Required]
    [MaxLength(255)]
    public string VariableName { get; set; }

    [Column("variable_value")]
    [Required]
    [MaxLength(255)]
    public string VariableValue { get; set; }
}
