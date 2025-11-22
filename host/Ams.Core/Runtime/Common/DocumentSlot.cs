namespace Ams.Core.Runtime.Common;

internal sealed class DocumentSlot<T>
    where T : class
{
    private readonly Func<T?> _loader;
    private readonly Action<T> _saver;
    private readonly Func<T?, T?>? _postLoadTransform;
    private readonly bool _writeThrough;
    private readonly Func<FileInfo?>? _backingFileAccessor;
    private readonly IDocumentSlotAdapter<T>? _adapter;
    private bool _loaded;
    private bool _dirty;
    private T? _value;

    public DocumentSlot(Func<T?> loader, Action<T> saver, DocumentSlotOptions<T>? options = null)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _saver = saver ?? throw new ArgumentNullException(nameof(saver));

        if (options is not null)
        {
            _postLoadTransform = options.PostLoadTransform;
            _backingFileAccessor = options.BackingFileAccessor;
            _writeThrough = options.WriteThrough;
        }
    }

    public DocumentSlot(IDocumentSlotAdapter<T> adapter, DocumentSlotOptions<T>? options = null)
        : this(
            (adapter ?? throw new ArgumentNullException(nameof(adapter))).Load,
            adapter.Save,
            options)
    {
        _adapter = adapter;
    }

    public bool IsDirty => _dirty;

    public T? GetValue()
    {
        if (!_loaded)
        {
            var loaded = _loader();
            if (_postLoadTransform is not null)
            {
                loaded = _postLoadTransform(loaded);
            }

            _value = loaded;
            _loaded = true;
        }

        return _value;
    }

    public void SetValue(T? value) => SetValue(value, markClean: false);

    public void SetValue(T? value, bool markClean)
    {
        _value = value;
        _loaded = true;

        if (markClean)
        {
            _dirty = false;
            return;
        }

        if (_writeThrough && value is not null)
        {
            _saver(value);
            _dirty = false;
            return;
        }

        _dirty = value is not null;
    }

    public void Invalidate(bool keepDirty = false)
    {
        _loaded = false;
        _value = null;
        if (!keepDirty)
        {
            _dirty = false;
        }
    }

    public FileInfo? GetBackingFile()
    {
        if (_backingFileAccessor is not null)
        {
            return _backingFileAccessor();
        }

        return _adapter?.GetBackingFile();
    }

    public void Save()
    {
        if (!_dirty)
        {
            return;
        }

        if (_value is null)
        {
            _dirty = false;
            return;
        }

        _saver(_value);
        _dirty = false;
    }
}