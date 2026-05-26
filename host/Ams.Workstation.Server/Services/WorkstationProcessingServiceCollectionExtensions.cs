using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa;
using Ams.Core.Application.Validation;
using Ams.Core.Runtime.Book;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Documents;
using Ams.Core.Services.Interfaces;
using Ams.Workstation.Server.Services.Prep;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ams.Workstation.Server.Services;

public static class WorkstationProcessingServiceCollectionExtensions
{
    public static IServiceCollection AddWorkstationProcessingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IPronunciationProvider>(_ => new MfaPronunciationProvider());
        services.TryAddSingleton<IBookCache>(_ => new BookCache());
        services.TryAddSingleton<IDocumentService, DocumentService>();
        services.TryAddSingleton<IAsrService, AsrService>();
        services.TryAddSingleton<IAnchorComputeService, AnchorComputeService>();
        services.TryAddSingleton<ITranscriptIndexService, TranscriptIndexService>();
        services.TryAddSingleton<ITranscriptHydrationService, TranscriptHydrationService>();
        services.TryAddSingleton<IAlignmentService, AlignmentService>();
        services.TryAddSingleton<GenerateTranscriptCommand>();
        services.TryAddSingleton<ComputeAnchorsCommand>();
        services.TryAddSingleton<BuildBookIndexCommand>();
        services.TryAddSingleton<BuildTranscriptIndexCommand>();
        services.TryAddSingleton<HydrateTranscriptCommand>();
        services.TryAddSingleton<RunMfaCommand>();
        services.TryAddSingleton<MergeTimingsCommand>();
        services.TryAddSingleton<PipelineService>();
        services.TryAddSingleton<ValidationService>();
        services.TryAddSingleton<IPrepRuntimeReadinessProbe, PrepRuntimeReadinessProbe>();

        return services;
    }
}
