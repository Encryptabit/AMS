using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Interfaces;

public interface IBookManager
{
    int Count { get; }
    BookContext Current { get; }
    BookContext Load(int index);
    BookContext Load(string bookId);
    bool TryMoveNext(out BookContext context);
    bool TryMovePrevious(out BookContext context);
    void Reset();
    void Deallocate(string bookId);
    void DeallocateAll();
}