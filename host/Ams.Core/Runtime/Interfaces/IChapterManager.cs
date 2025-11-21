using System.IO;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Interfaces;

public interface IChapterManager
{
    int Count { get; }
    IReadOnlyList<ChapterDescriptor> Descriptors { get; }
    ChapterContext Current { get; }
    ChapterContext Load(int index);
    ChapterContext Load(string chapterId);
    bool Contains(string chapterId);
    ChapterContextHandle CreateContext(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null,
        bool reloadBookIndex = false);
    ChapterDescriptor UpsertDescriptor(ChapterDescriptor descriptor);
    bool TryMoveNext(out ChapterContext context);
    bool TryMovePrevious(out ChapterContext context);
    void Reset();
    void Deallocate(string chapterId);
    void DeallocateAll();
}
