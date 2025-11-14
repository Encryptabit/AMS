using System;

namespace Ams.Core.Runtime.Common;

internal sealed class DocumentSlot<T>
    where T : class
{
    private readonly Func<T?> _loader;
    private readonly Action<T> _saver;
    private bool _loaded;
    private bool _dirty;
    private T? _value;

    public DocumentSlot(Func<T?> loader, Action<T> saver)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _saver = saver ?? throw new ArgumentNullException(nameof(saver));
    }

    public bool IsDirty => _dirty;

    public T? GetValue()
    {
        if (!_loaded)
        {
            _value = _loader();
            _loaded = true;
        }

        return _value;
    }

    public void SetValue(T? value)
    {
        _value = value;
        _loaded = true;
        _dirty = true;
    }

    public void Invalidate()
    {
        _loaded = false;
        _value = null;
        _dirty = false;
    }

    public void Save()
    {
        if (!_dirty || _value is null)
        {
            _dirty = false;
            return;
        }

        _saver(_value);
        _dirty = false;
    }
}
