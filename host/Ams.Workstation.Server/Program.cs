using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;
using Ams.Workstation.Server.Components;
using Ams.Workstation.Server.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHotKeys2();

// Blazor workspace - singleton for single-user workstation
// Allows API controllers and Blazor circuits to share the same state
builder.Services.AddSingleton<BlazorWorkspace>();

// Chapter data service - singleton (reads from workspace)
builder.Services.AddSingleton<ChapterDataService>();

// Proof/Validation services - transient (stateless computation)
builder.Services.AddTransient<ValidationMetricsService>();
builder.Services.AddTransient<ProofReportService>();
builder.Services.AddTransient<ErrorPatternService>();

// Persistence services - singleton (shared state across circuits)
builder.Services.AddSingleton<ReviewedStatusService>();

// Ams.Core services - stateless services for alignment/ASR operations
// Note: PipelineService and ValidationService require command dependencies
// that are CLI-specific. Add them when needed with proper command registration.
builder.Services.AddSingleton<IAsrService, AsrService>();
builder.Services.AddSingleton<IAnchorComputeService, AnchorComputeService>();
builder.Services.AddSingleton<ITranscriptIndexService, TranscriptIndexService>();
builder.Services.AddSingleton<ITranscriptHydrationService, TranscriptHydrationService>();
builder.Services.AddSingleton<IAlignmentService, AlignmentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
