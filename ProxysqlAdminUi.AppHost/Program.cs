var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ProxysqlAdminUi_ApiService>("apiservice");

builder.AddProject<Projects.ProxysqlAdminUi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
