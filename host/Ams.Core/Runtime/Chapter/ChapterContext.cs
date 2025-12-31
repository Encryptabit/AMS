using Ams.Core.Common;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Alignment;
using Ams.Core.Processors.Alignment.Anchors;
using Microsoft.Extensions.Logging;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterContext
{
    private readonly IArtifactResolver _resolver;
    private SectionRange? _resolvedSection;

    internal ChapterContext(BookContext book, ChapterDescriptor descriptor)
    {
        Book = book ?? throw new ArgumentNullException(nameof(book));
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _resolver = book.Resolver;
        Documents = new ChapterDocuments(this, _resolver);
        Audio = new AudioBufferManager(descriptor.AudioBuffers);
    }

    public BookContext Book { get; }
    public ChapterDescriptor Descriptor { get; }
    public ChapterDocuments Documents { get; }
    public AudioBufferManager Audio { get; }

    public void Save()
    {
        Documents.SaveChanges();
    }

    public FileInfo ResolveArtifactFile(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw new ArgumentException("Suffix must be provided.", nameof(suffix));
        }

        var trimmedSuffix = suffix.Trim().TrimStart('.');
        if (trimmedSuffix.Length == 0)
        {
            throw new ArgumentException("Suffix must contain file information.", nameof(suffix));
        }

        return _resolver.GetChapterArtifactFile(this, trimmedSuffix);
    }

    /// <summary>
    /// Resolve and cache the book section for this chapter (override > label mapping; auto-detect handled by caller).
    /// </summary>
    internal SectionRange? GetOrResolveSection(BookIndex book, AnchorComputationOptions options, string stage,
        ILogger logger)
    {
        if (_resolvedSection is not null)
        {
            logger.LogInformation(
                "Resolved section ({Stage}) from cache for {ChapterId}: {Title} (Id={Id}, Words={Start}-{End})",
                stage,
                Descriptor.ChapterId,
                _resolvedSection.Title,
                _resolvedSection.Id,
                _resolvedSection.StartWord,
                _resolvedSection.EndWord);
            return _resolvedSection;
        }

        if (options.SectionOverride is not null)
        {
            _resolvedSection = options.SectionOverride;
            return _resolvedSection;
        }

        if (options.TryResolveSectionFromLabels)
        {
            foreach (var label in ChapterLabelResolver.EnumerateLabelCandidates(
                Descriptor.ChapterId, Descriptor.RootPath))
            {
                if (ChapterLabelResolver.TryExtractChapterNumber(label, out var numberFromLabel))
                {
                    var numericSection = SectionLocator.ResolveSectionByTitle(book, numberFromLabel.ToString());
                    if (numericSection is not null)
                    {
                        _resolvedSection = numericSection;
                        logger.LogInformation(
                            "Resolved section ({Stage}) from numeric label '{Label}' → {Number} for {ChapterId}: {Title} (Id={Id}, Words={Start}-{End})",
                            stage,
                            label,
                            numberFromLabel,
                            Descriptor.ChapterId,
                            numericSection.Title,
                            numericSection.Id,
                            numericSection.StartWord,
                            numericSection.EndWord);
                        return _resolvedSection;
                    }
                }

                var section = SectionLocator.ResolveSectionByTitle(book, label);
                if (section is not null)
                {
                    _resolvedSection = section;
                    logger.LogInformation(
                        "Resolved section ({Stage}) from label '{Label}' for {ChapterId}: {Title} (Id={Id}, Words={Start}-{End})",
                        stage,
                        label,
                        Descriptor.ChapterId,
                        section.Title,
                        section.Id,
                        section.StartWord,
                        section.EndWord);
                    return _resolvedSection;
                }
            }
        }

        logger.LogDebug("Section not resolved from labels for {ChapterId}; will rely on auto-detect",
            Descriptor.ChapterId);
        return null;
    }

    internal void SetDetectedSection(SectionRange section)
    {
        _resolvedSection ??= section;
    }
}
