using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterContext
{
    private readonly IArtifactResolver _resolver;

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
}