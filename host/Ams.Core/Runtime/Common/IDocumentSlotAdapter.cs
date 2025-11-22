namespace Ams.Core.Runtime.Common;

internal interface IDocumentSlotAdapter<T>
    where T : class
{
    T? Load();
    void Save(T document);
    FileInfo? GetBackingFile();
}