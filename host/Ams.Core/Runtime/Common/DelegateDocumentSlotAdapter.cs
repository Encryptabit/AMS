namespace Ams.Core.Runtime.Common;

internal sealed class DelegateDocumentSlotAdapter<T> : IDocumentSlotAdapter<T>
    where T : class
{
    private readonly Func<T?> _loader;
    private readonly Action<T> _saver;
    private readonly Func<FileInfo?>? _backingFileAccessor;

    public DelegateDocumentSlotAdapter(
        Func<T?> loader,
        Action<T> saver,
        Func<FileInfo?>? backingFileAccessor = null)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _saver = saver ?? throw new ArgumentNullException(nameof(saver));
        _backingFileAccessor = backingFileAccessor;
    }

    public T? Load() => _loader();

    public void Save(T document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _saver(document);
    }

    public FileInfo? GetBackingFile() => _backingFileAccessor?.Invoke();
}