using Ams.Core.Runtime.Book;

namespace Ams.Core.Processors.DocumentProcessor;

public static partial class DocumentProcessor
{
    public static Task<BookIndex> PopulateMissingPhonemesAsync(
        BookIndex index,
        IPronunciationProvider pronunciationProvider,
        CancellationToken cancellationToken = default)
    {
        return BookPhonemePopulator.PopulateMissingAsync(index, pronunciationProvider, cancellationToken);
    }
}
