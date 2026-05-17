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
    private DocumentSlotState<T> _state = new NotLoaded<T>();

    public DocumentSlot(Func<T?> loader, Action<T> saver, DocumentSlotOptions<T>? options = null)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _saver = saver ?? throw new ArgumentNullException(nameof(saver));

        if (options is null) return;

        _postLoadTransform = options.PostLoadTransform;
        _backingFileAccessor = options.BackingFileAccessor;
        _writeThrough = options.WriteThrough;
    }

    public DocumentSlot(IDocumentSlotAdapter<T> adapter, DocumentSlotOptions<T>? options = null)
        : this(
            (adapter ?? throw new ArgumentNullException(nameof(adapter))).Load,
            adapter.Save,
            options)
    {
        _adapter = adapter;
    }

    public bool IsDirty => _state.IsDirty;

    internal string StateName => _state.Name;

    public T? GetValue()
    {
        return _state switch
        {
            LoadedClean<T> loaded => loaded.Value,
            LoadedDirty<T> dirty => dirty.Value,
            LoadedMissing<T> => null,
            Invalidated<T> invalidated => LoadValue(markDirty: invalidated.PreserveDirty),
            NotLoaded<T> => LoadValue(markDirty: false),
            _ => throw new InvalidOperationException($"Unknown document slot state '{_state.GetType().Name}'.")
        };
    }

    public void SetValue(T? value) => SetValue(value, markClean: false);

    public void SetValue(T? value, bool markClean)
    {
        if (value is null)
        {
            _state = new LoadedMissing<T>();
            return;
        }

        if (markClean)
        {
            _state = new LoadedClean<T>(value);
            return;
        }

        if (_writeThrough)
        {
            _saver(value);
            _state = new LoadedClean<T>(value);
            return;
        }

        _state = new LoadedDirty<T>(value);
    }

    public void Invalidate(bool keepDirty = false)
    {
        _state = new Invalidated<T>(keepDirty && _state.IsDirty);
    }

    public FileInfo? GetBackingFile()
    {
        return _backingFileAccessor is not null ? _backingFileAccessor() : _adapter?.GetBackingFile();
    }

    public void Save()
    {
        if (!_state.IsDirty)
        {
            return;
        }

        if (_state is not LoadedDirty<T> dirty)
        {
            _state = new NotLoaded<T>();
            return;
        }

        _saver(dirty.Value);
        _state = new LoadedClean<T>(dirty.Value);
    }

    private T? LoadValue(bool markDirty)
    {
        var loaded = _loader();
        if (_postLoadTransform is not null)
        {
            loaded = _postLoadTransform(loaded);
        }

        _state = loaded is null
            ? new LoadedMissing<T>()
            : markDirty
                ? new LoadedDirty<T>(loaded)
                : new LoadedClean<T>(loaded);

        return loaded;
    }
}

internal abstract record DocumentSlotState<T>
    where T : class
{
    public abstract string Name { get; }
    public abstract bool IsDirty { get; }
}

internal sealed record NotLoaded<T> : DocumentSlotState<T>
    where T : class
{
    public override string Name => "not-loaded";
    public override bool IsDirty => false;
}

internal sealed record LoadedMissing<T> : DocumentSlotState<T>
    where T : class
{
    public override string Name => "loaded-missing";
    public override bool IsDirty => false;
}

internal sealed record LoadedClean<T>(T Value) : DocumentSlotState<T>
    where T : class
{
    public override string Name => "loaded-clean";
    public override bool IsDirty => false;
}

internal sealed record LoadedDirty<T>(T Value) : DocumentSlotState<T>
    where T : class
{
    public override string Name => "loaded-dirty";
    public override bool IsDirty => true;
}

internal sealed record Invalidated<T>(bool PreserveDirty) : DocumentSlotState<T>
    where T : class
{
    public override string Name => PreserveDirty ? "invalidated-dirty" : "invalidated";
    public override bool IsDirty => PreserveDirty;
}
