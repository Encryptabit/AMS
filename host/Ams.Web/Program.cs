using Ams.Core.Application.Commands;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Web.Components;
using Ams.Web.Configuration;
using Ams.Web.Endpoints;
using Ams.Web.Client;
using Ams.Web.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AmsOptions>()
    .Bind(builder.Configuration.GetSection(AmsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Core service graph mirrors the CLI host so orchestration remains identical.
builder.Services.AddSingleton<IAsrService, AsrService>();
builder.Services.AddSingleton<IAlignmentService, AlignmentService>();
builder.Services.AddSingleton<GenerateTranscriptCommand>();
builder.Services.AddSingleton<ComputeAnchorsCommand>();
builder.Services.AddSingleton<BuildTranscriptIndexCommand>();
builder.Services.AddSingleton<HydrateTranscriptCommand>();
builder.Services.AddSingleton<RunMfaCommand>();
builder.Services.AddSingleton<MergeTimingsCommand>();
builder.Services.AddSingleton<PipelineService>();
builder.Services.AddSingleton<ValidationService>();

// Web-specific services
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});
builder.Services.AddScoped<WorkspaceApiClient>();
builder.Services.AddScoped<ChapterApiClient>();
builder.Services.AddSingleton<WorkspaceState>();
builder.Services.AddSingleton<WebWorkspace>();
builder.Services.AddSingleton<IWorkspace>(sp => sp.GetRequiredService<WebWorkspace>());
builder.Services.AddSingleton<ChapterContextAccessor>();
builder.Services.AddSingleton<ChapterCatalogService>();
builder.Services.AddSingleton<ChapterDataService>();
builder.Services.AddSingleton<ReviewedStatusStore>();
builder.Services.AddSingleton<IAudioSegmentExporter, FfmpegCliSegmentExporter>();
builder.Services.AddSingleton<CrxExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapChapterApi();
app.MapWorkspaceApi();

app.Run();
