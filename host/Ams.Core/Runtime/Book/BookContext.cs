using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Book;

public sealed class BookContext
{
    internal BookContext(BookDescriptor descriptor)
    {
        Descriptor = descriptor;
        Documents = new BookDocumentManager(descriptor.Documents);
        Chapters = new ChapterManager(this);
    }

    public BookDescriptor Descriptor { get; }
    public BookDocumentManager Documents { get; }
    public ChapterManager Chapters { get; }
}
