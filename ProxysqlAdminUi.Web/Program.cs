using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProxysqlAdminUi.Web.Components;
using ProxysqlAdminUi.Web.Data;
using ProxysqlAdminUi.Web.Components.Account;
using ProxysqlAdminUi.Web.Contexts;
using ProxysqlAdminUi.Web.Repositories;
using ProxysqlAdminUi.Web.Services;

var builder = WebApplication.CreateBuilder(args);

#region Services

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();


builder.Services.AddIdentityCore<ProxysqlAdminUiWebUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;
    })
    .AddEntityFrameworkStores<ProxysqlAdminUiWebAuthContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.SetupIdentityDbContext();

var proxySqlConnectionString = builder.Configuration.GetConnectionString("ProxySqlContext") ?? throw new InvalidOperationException("Connection string 'ProxySqlContext' not found.");
builder.Services.AddDbContext<ProxySqlContext>(options =>
    options.UseMySQL(proxySqlConnectionString));

builder.Services.AddScoped<ProxySqlRepository>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSingleton<IEmailSender<ProxysqlAdminUiWebUser>, IdentityNoOpEmailSender>();

builder.Services.AddOutputCache();

builder.Services.AddScoped<DefaultUserSeedService>();
#endregion

#region App

var app = builder.Build();


var scope = app.Services.CreateAsyncScope();

await scope.ServiceProvider.GetService<DefaultUserSeedService>().SeedDefaultUsersAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.UseSwagger();
app.UseSwaggerUI();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
#endregion