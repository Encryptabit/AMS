using System.IO;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Web.Services;

public sealed class ChapterDataService
{
    private readonly ChapterContextAccessor _contextAccessor;

    public ChapterDataService(ChapterContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    public Task<HydratedTranscript> LoadHydratedTranscriptAsync(ChapterSummary summary, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(summary);
        cancellationToken.ThrowIfCancellationRequested();

        var chapter = _contextAccessor.GetChapter(summary);
        var document = chapter.Documents.HydratedTranscript
                       ?? throw new InvalidOperationException($"Hydrated transcript is not available for chapter '{summary.Id}'.");

        return Task.FromResult(document);
    }

    public Task<FileInfo> ResolveAudioFileAsync(ChapterSummary summary, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(summary);
        cancellationToken.ThrowIfCancellationRequested();

        var chapter = _contextAccessor.GetChapter(summary);
        var descriptor = ResolveAudioDescriptor(chapter.Audio);
        if (!File.Exists(descriptor.Path))
        {
            throw new FileNotFoundException($"Audio buffer '{descriptor.Path}' was not found.", descriptor.Path);
        }

        return Task.FromResult(new FileInfo(descriptor.Path));
    }

    private static AudioBufferDescriptor ResolveAudioDescriptor(AudioBufferManager manager)
    {
        if (manager.Count == 0)
        {
            throw new InvalidOperationException("Chapter does not define any audio buffers.");
        }

        if (TryLoadBuffer(manager, "raw", out var descriptor))
        {
            return descriptor;
        }

        return manager.Current.Descriptor;
    }

    private static bool TryLoadBuffer(AudioBufferManager manager, string bufferId, out AudioBufferDescriptor descriptor)
    {
        try
        {
            descriptor = manager.Load(bufferId).Descriptor;
            return true;
        }
        catch
        {
            descriptor = default!;
            return false;
        }
    }
}
