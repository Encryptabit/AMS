using Ams.Core.Artifacts;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Audio;

public sealed class AudioBufferContext
{
    private readonly AudioBufferDescriptor _descriptor;
    private readonly Func<AudioBufferDescriptor, AudioBuffer?> _loader;
    private AudioBuffer? _buffer;
    private bool _loaded;

    internal AudioBufferContext(AudioBufferDescriptor descriptor, Func<AudioBufferDescriptor, AudioBuffer?> loader)
    {
        _descriptor = descriptor;
        _loader = loader;
    }

    public AudioBufferDescriptor Descriptor => _descriptor;

    public AudioBuffer? Buffer
    {
        get
        {
            if (!_loaded)
            {
                _buffer = _loader(_descriptor);
                _loaded = true;
                Log.Debug(
                    "AudioBufferContext loaded buffer {BufferId} (has data: {HasData})",
                    _descriptor.BufferId,
                    _buffer is not null);
            }

            return _buffer;
        }
    }

    public bool IsLoaded => _loaded && _buffer != null;

    public void Unload()
    {
        _buffer = null;
        _loaded = false;
        Log.Debug("AudioBufferContext unloaded buffer {BufferId}", _descriptor.BufferId);
    }
}
