using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterContext
{
    internal ChapterContext(BookContext book, ChapterDescriptor descriptor)
    {
        Book = book;
        Descriptor = descriptor;
        Documents = new ChapterDocumentManager(descriptor.Documents);
        Audio = new AudioBufferManager(descriptor.AudioBuffers);
    }

    public BookContext Book { get; }
    public ChapterDescriptor Descriptor { get; }
    public ChapterDocumentManager Documents { get; }
    public AudioBufferManager Audio { get; }
}
