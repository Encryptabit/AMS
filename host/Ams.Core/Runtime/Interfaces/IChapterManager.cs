using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public interface IChapterManager
{
    int Count { get; }
    IReadOnlyList<ChapterDescriptor> Descriptors { get; }
    ChapterContext Current { get; }
    ChapterContext Load(int index);
    ChapterContext Load(string chapterId);
    bool Contains(string chapterId);
    ChapterDescriptor UpsertDescriptor(ChapterDescriptor descriptor);
    bool TryMoveNext(out ChapterContext context);
    bool TryMovePrevious(out ChapterContext context);
    void Reset();
    void Deallocate(string chapterId);
    void DeallocateAll();
}