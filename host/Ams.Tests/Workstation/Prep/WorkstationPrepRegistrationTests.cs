using Ams.Core.Application.Commands;
using Ams.Core.Application.Validation;
using Ams.Core.Runtime.Book;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Prep;
using Microsoft.Extensions.DependencyInjection;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepRegistrationTests
{
    [Fact]
    public void AddWorkstationProcessingServices_ResolvesSharedPrepGraph()
    {
        var services = new ServiceCollection();
        services.AddWorkstationProcessingServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        Assert.NotNull(provider.GetRequiredService<BuildBookIndexCommand>());
        Assert.NotNull(provider.GetRequiredService<PipelineService>());
        Assert.NotNull(provider.GetRequiredService<ValidationService>());
        Assert.NotNull(provider.GetRequiredService<IPrepRuntimeReadinessProbe>());
    }

    [Fact]
    public void AddWorkstationProcessingServices_RegistersExpectedSharedProcessingGraphAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddWorkstationProcessingServices();

        foreach (var serviceType in ExpectedSingletonServiceTypes)
        {
            AssertSingleDescriptor(services, serviceType, ServiceLifetime.Singleton);
        }

        using var provider = services.BuildServiceProvider();
        Assert.Same(provider.GetRequiredService<BuildBookIndexCommand>(), provider.GetRequiredService<BuildBookIndexCommand>());
        Assert.Same(provider.GetRequiredService<PipelineService>(), provider.GetRequiredService<PipelineService>());
        Assert.Same(provider.GetRequiredService<ValidationService>(), provider.GetRequiredService<ValidationService>());
        Assert.Same(provider.GetRequiredService<IPrepRuntimeReadinessProbe>(), provider.GetRequiredService<IPrepRuntimeReadinessProbe>());
    }

    [Fact]
    public void AddWorkstationProcessingServices_IsIdempotentAcrossRepeatedRegistrationCallsAndProviderBuilds()
    {
        var services = new ServiceCollection();
        services.AddWorkstationProcessingServices();
        services.AddWorkstationProcessingServices();

        foreach (var serviceType in ExpectedSingletonServiceTypes)
        {
            AssertSingleDescriptor(services, serviceType, ServiceLifetime.Singleton);
        }

        using var firstProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var secondProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        Assert.NotSame(firstProvider.GetRequiredService<BuildBookIndexCommand>(), secondProvider.GetRequiredService<BuildBookIndexCommand>());
        Assert.NotSame(firstProvider.GetRequiredService<PipelineService>(), secondProvider.GetRequiredService<PipelineService>());
        Assert.NotSame(firstProvider.GetRequiredService<ValidationService>(), secondProvider.GetRequiredService<ValidationService>());
        Assert.NotSame(firstProvider.GetRequiredService<IPrepRuntimeReadinessProbe>(), secondProvider.GetRequiredService<IPrepRuntimeReadinessProbe>());
    }

    [Fact]
    public void AddWorkstationProcessingServices_DoesNotRegisterPrepUiStateServices()
    {
        var services = new ServiceCollection();
        services.AddWorkstationProcessingServices();

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(PrepRunSession));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(PrepPipelineOperatorControls));
    }

    [Fact]
    public void PrepRunSession_CanResolveAsTransientWithSharedProcessingGraph()
    {
        var services = new ServiceCollection();
        services.AddSingleton<BlazorWorkspace>();
        services.AddWorkstationProcessingServices();
        services.AddTransient<PrepRunSession>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        using var first = provider.GetRequiredService<PrepRunSession>();
        using var second = provider.GetRequiredService<PrepRunSession>();

        Assert.NotSame(first, second);
        Assert.Same(provider.GetRequiredService<PipelineService>(), provider.GetRequiredService<PipelineService>());
    }

    private static IReadOnlyList<Type> ExpectedSingletonServiceTypes { get; } =
    [
        typeof(IPronunciationProvider),
        typeof(IBookCache),
        typeof(IDocumentService),
        typeof(IAsrService),
        typeof(IAnchorComputeService),
        typeof(ITranscriptIndexService),
        typeof(ITranscriptHydrationService),
        typeof(IAlignmentService),
        typeof(GenerateTranscriptCommand),
        typeof(ComputeAnchorsCommand),
        typeof(BuildBookIndexCommand),
        typeof(BuildTranscriptIndexCommand),
        typeof(HydrateTranscriptCommand),
        typeof(RunMfaCommand),
        typeof(MergeTimingsCommand),
        typeof(PipelineService),
        typeof(ValidationService),
        typeof(IPrepRuntimeReadinessProbe)
    ];

    private static void AssertSingleDescriptor(IServiceCollection services, Type serviceType, ServiceLifetime expectedLifetime)
    {
        var descriptor = Assert.Single(services, candidate => candidate.ServiceType == serviceType);
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }
}
