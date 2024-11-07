using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.ViewModel;

public class QueryDigestViewModel : StatsMysqlQueryDigest
{
    public bool HasRule { get; set; }
    public int? RuleId { get; set; }
}