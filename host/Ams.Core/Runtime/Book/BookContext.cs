using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Book;

public sealed class BookContext
{
    private readonly IArtifactResolver _resolver;

    internal BookContext(BookDescriptor descriptor, IArtifactResolver resolver)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        Documents = new BookDocuments(this, _resolver);
        Chapters = new ChapterManager(this);
    }

    internal IArtifactResolver Resolver => _resolver;

    public BookDescriptor Descriptor { get; }
    public BookDocuments Documents { get; }
    public ChapterManager Chapters { get; }

    public void Save()
    {
        Documents.SaveChanges();
    }

    public FileInfo ResolveArtifactFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided.", nameof(fileName));
        }

        return _resolver.GetBookArtifactFile(this, fileName.Trim());
    }
}
