using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.ViewModel;

public class QueryDigestViewModel : StatsMysqlQueryDigestModel
{
    public bool HasRule { get; set; }
    public int? RuleId { get; set; }
}