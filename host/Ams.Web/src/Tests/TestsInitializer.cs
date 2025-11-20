using Microsoft.EntityFrameworkCore;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Aspire.Hosting.DevTunnels;
using Aspire.Hosting.ApplicationModel;
using Ams.Web.Server.Api.Data;
using Microsoft.Extensions.Hosting;

namespace Ams.Web.Tests;

[TestClass]
public partial class TestsInitializer
{
    private static DistributedApplication? aspireApp;

    [AssemblyInitialize]
    public static async Task Initialize(TestContext testContext)
    {
        await RunAspireHost(testContext);
        await using var testServer = new AppTestServer();

        await testServer.Build().Start(testContext.CancellationToken);

        await InitializeDatabase(testServer);
    }

    /// <summary>
    /// Aspire.Hosting.Testing executes the complete application, including dependencies like databases, 
    /// closely mimicking a production environment. However, it has a limitation: backend services cannot 
    /// be overridden in tests if needed, unlike <see cref="AppTestServer"/> used in <see cref="IdentityApiTests"/> 
    /// and <see cref="IdentityPagesTests"/>. The code below runs the Aspire app without the server web 
    /// project, retrieves necessary connection strings (e.g., database connection string), and passes 
    /// them to <see cref="AppTestServer"/>.
    /// </summary>
    private static async Task RunAspireHost(TestContext testContext)
    {
        var aspireBuilder = await DistributedApplicationTestingBuilder
            .CreateAsync<Program>(testContext.CancellationToken);

        foreach (var res in aspireBuilder.Resources.OfType<ProjectResource>().ToList())
            aspireBuilder.Resources.Remove(res);
        foreach (var res in aspireBuilder.Resources.OfType<DevTunnelResource>().ToList()) // remove unnecessary resources.
            aspireBuilder.Resources.Remove(res);

        aspireApp = await aspireBuilder.BuildAsync(testContext.CancellationToken);

        await aspireApp.StartAsync(testContext.CancellationToken);

        Environment.SetEnvironmentVariable("ConnectionStrings__mssqldb", await aspireApp.GetConnectionStringAsync("mssqldb", testContext.CancellationToken));
        await aspireApp.ResourceNotifications.WaitForResourceAsync("mssqldb", KnownResourceStates.Running, testContext.CancellationToken);
        Environment.SetEnvironmentVariable("ConnectionStrings__azureblobstorage", await aspireApp.GetConnectionStringAsync("azureblobstorage", testContext.CancellationToken));
        await aspireApp.ResourceNotifications.WaitForResourceAsync("azureblobstorage", KnownResourceStates.Running, testContext.CancellationToken);
        Environment.SetEnvironmentVariable("ConnectionStrings__smtp", await aspireApp.GetConnectionStringAsync("smtp", testContext.CancellationToken));
        await aspireApp.ResourceNotifications.WaitForResourceAsync("smtp", KnownResourceStates.Running, testContext.CancellationToken);
    }

    private static async Task InitializeDatabase(AppTestServer testServer)
    {
        if (testServer.WebApp.Environment.IsDevelopment())
        {
            await using var scope = testServer.WebApp.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync(); // It's recommended to start using ef-core migrations.
        }
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        if (aspireApp is not null)
        {
            await aspireApp.StopAsync();
            await aspireApp.DisposeAsync();
        }
    }
}
