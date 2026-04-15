using Ams.Core.Application.Processes;
using Ams.Workstation.Server.Components;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;
using Ams.Workstation.Server.Services.Prep;
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

// Toast notification service - singleton (shared across circuits)
builder.Services.AddSingleton<ToastService>();

// Persistence services - singleton (shared state across circuits)
builder.Services.AddSingleton<ReviewedStatusService>();
builder.Services.AddSingleton<IgnoredPatternsService>();
builder.Services.AddSingleton<WorkspaceHistoryService>();
builder.Services.AddSingleton<BookMetadataResetService>();

// Audio export and CRX services - transient (stateless, uses workspace for audio access)
builder.Services.AddTransient<AudioExportService>();
builder.Services.AddTransient<CrxService>();

// Polish area services - singleton (shared state, persistent queue and undo storage)
builder.Services.AddSingleton<StagingQueueService>();
builder.Services.AddSingleton<UndoService>();
builder.Services.AddSingleton<PreviewBufferService>();
builder.Services.AddSingleton<EditListService>();
builder.Services.AddSingleton<PickupEdlStore>();
builder.Services.AddSingleton<PickupEdlEngine>();
builder.Services.AddSingleton<PickupSourceBufferCache>();

// Polish area services - transient (stateless matching and orchestration)
builder.Services.AddTransient<PickupMfaRefinementService>();
builder.Services.AddTransient<PickupMatchingService>();
builder.Services.AddTransient<PickupAssetService>();
builder.Services.AddTransient<PolishService>();
builder.Services.AddTransient<PolishVerificationService>();
builder.Services.AddTransient<BatchOperationService>();
builder.Services.AddScoped<ProofPickupsSessionService>();

// Ams.Core processing graph for Prep and other shared execution flows.
builder.Services.AddWorkstationProcessingServices();
builder.Services.AddTransient<PrepRunSession>();

var app = builder.Build();

// Warm MFA conda environment in the background so forced alignment is ready
// by the time pickup imports run (avoids first-use latency).
MfaProcessSupervisor.TriggerBackgroundWarmup();

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
