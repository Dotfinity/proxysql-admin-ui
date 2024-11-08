using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Data;

namespace ProxysqlAdminUi.Web.Contexts;

public class ProxysqlAdminUiWebAuthContext(DbContextOptions<ProxysqlAdminUiWebAuthContext> options) : IdentityDbContext<ProxysqlAdminUiWebUser>(options);

