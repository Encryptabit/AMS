using Hangfire;
using Ams.Web.Server.Web;
using Ams.Web.Tests.Services;
using Ams.Web.Server.Api.Services;
using Ams.Web.Client.Core.Services.HttpMessageHandlers;

namespace Microsoft.AspNetCore.Builder;

public static partial class WebApplicationBuilderExtensions
{
    public static void AddTestProjectServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddServerWebProjectServices();

        // Register test-specific services for all tests here

        services.AddTransient<HttpClient>(sp =>
        {
            var handlerFactory = sp.GetRequiredService<HttpMessageHandlersChainFactory>();
            return new HttpClient(handlerFactory.Invoke())
            {
                BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>().GetServerAddress(), UriKind.Absolute)
            };
        });
    }
}
