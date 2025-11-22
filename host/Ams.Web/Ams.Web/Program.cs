using Ams.Web.Components;
using Ams.Web.Client.Pages;
using Ams.Web.Client.Services;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();



builder.Services.AddBitBlazorUIServices();
builder.Services.AddScoped(sp =>
{
    // Use the current request's base URI so the client can call relative API endpoints during prerender.
    var navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});
builder.Services.AddScoped<ValidationApiClient>();

var app = builder.Build();

// Support hosting under a path base (e.g., /ams) without requiring env config.
var configuredPathBase = builder.Configuration["PathBase"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
app.Use((ctx, next) =>
{
    var effectivePathBase = configuredPathBase;
    PathString remaining;

    // Auto-detect /ams if not configured
    if (string.IsNullOrWhiteSpace(effectivePathBase) && ctx.Request.Path.StartsWithSegments("/ams", out remaining))
    {
        effectivePathBase = "/ams";
    }

    if (string.IsNullOrWhiteSpace(effectivePathBase) is false &&
        ctx.Request.Path.StartsWithSegments(effectivePathBase, out remaining))
    {
        ctx.Request.PathBase = effectivePathBase;
        ctx.Request.Path = remaining;
    }

    return next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (builder.Environment.IsDevelopment() is false)
{
    app.UseHttpsRedirection();
    app.UseResponseCompression();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Ams.Web.Client._Imports).Assembly);

app.Run();