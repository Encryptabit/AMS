using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Common;

namespace Ams.Core.Runtime.Book;

public sealed class BookDocuments
{
    private readonly DocumentSlot<BookIndex> _bookIndex;

    internal BookDocuments(BookContext context, IArtifactResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(resolver);

        _bookIndex = new DocumentSlot<BookIndex>(
            () => resolver.LoadBookIndex(context),
            value => resolver.SaveBookIndex(context, value),
            new DocumentSlotOptions<BookIndex>
            {
                BackingFileAccessor = () => resolver.GetBookIndexFile(context)
            });
    }

    public BookIndex? BookIndex
    {
        get => _bookIndex.GetValue();
        set => _bookIndex.SetValue(value);
    }

    internal void SetLoadedBookIndex(BookIndex? bookIndex)
        => _bookIndex.SetValue(bookIndex, markClean: true);

    internal void SaveChanges() => _bookIndex.Save();

    internal FileInfo? GetBookIndexFile() => _bookIndex.GetBackingFile();
}
