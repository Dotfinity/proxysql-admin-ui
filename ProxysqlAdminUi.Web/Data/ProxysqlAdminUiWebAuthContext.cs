using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProxysqlAdminUi.Web.Data
{
    public class ProxysqlAdminUiWebAuthContext(DbContextOptions<ProxysqlAdminUiWebAuthContext> options) : IdentityDbContext<ProxysqlAdminUiWebUser>(options)
    {
    }
}
