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

// Blazor workspace - scoped per circuit
builder.Services.AddScoped<BlazorWorkspace>();

// Chapter data service - scoped per circuit for sentence loading
builder.Services.AddScoped<ChapterDataService>();

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
