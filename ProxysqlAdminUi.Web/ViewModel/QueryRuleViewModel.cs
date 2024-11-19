using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.ViewModel;

public class QueryRuleViewModel : MysqlQueryRuleModel
{
    // Additional stats properties
    public long Hits { get; set; } = 0;
    public string? DigestText { get; set; }

    public long CountStar { get; set; } = 0;
}