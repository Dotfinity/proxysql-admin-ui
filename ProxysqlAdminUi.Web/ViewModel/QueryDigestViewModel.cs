using ProxysqlAdminUi.Web.Models;

namespace ProxysqlAdminUi.Web.ViewModel;

public class QueryDigestViewModel : StatsMysqlQueryDigestModel
{
    /// <summary>
    /// Indicates how many times this rule digest has hit it's cache vs non cache.
    /// NB: one rule can have many digests, example: if one rule matches by table prefix "ps_setting*"
    /// if there are 10 tables being accessed, via 20 queries, that would result in 40 different digests.
    /// 20 for the uncached queries and 20 for the cache hits. 
    /// </summary>
    public long? CacheHits { get; set; }
    public bool HasRule { get; set; }
    public int? RuleId { get; set; }
}