using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Check out appsettings.Development.json for credentials/passwords settings.

var sqlDatabase = builder.AddSqlServer("sqlserver")
        .WithDbGate(config => config.WithDataVolume())
        .WithDataVolume()
        .WithImage("mssql/server", "2025-latest")
        .AddDatabase("mssqldb"); // Sql server 2025 supports embedded vector search.

var azureBlobStorage = builder.AddAzureStorage("storage")
        .RunAsEmulator(azurite =>
        {
            azurite
                .WithDataVolume();
        })
        .AddBlobs("azureblobstorage");


var serverWebProject = builder.AddProject("serverweb", "../Ams.Web.Server.Web/Ams.Web.Server.Web.csproj")
    .WithExternalHttpEndpoints();

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.IsDevelopment())
{
    serverWebProject.WithHttpHealthCheck("/alive");
}


serverWebProject.WithReference(sqlDatabase).WaitFor(sqlDatabase);
serverWebProject.WithReference(azureBlobStorage);

if (builder.ExecutionContext.IsRunMode) // The following project is only added for testing purposes.
{
    // Blazor WebAssembly Standalone project.
    builder.AddProject("clientwebwasm", "../../Client/Ams.Web.Client.Web/Ams.Web.Client.Web.csproj")
        .WithExplicitStart();

    var mailpit = builder.AddMailPit("smtp") // For testing purposes only, in production, you would use a real SMTP server.
        .WithDataVolume("mailpit");

    serverWebProject.WithReference(mailpit);

    var tunnel = builder.AddDevTunnel("web-dev-tunnel")
        .WithAnonymousAccess()
        .WithReference(serverWebProject.WithHttpEndpoint(name: "devTunnel").GetEndpoint("devTunnel"));
}

await builder
    .Build()
    .RunAsync();
