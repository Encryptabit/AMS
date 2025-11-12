namespace Ams.Core.Runtime.Book;

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