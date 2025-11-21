using Ams.Core.Runtime.Audio;

namespace Ams.Core.Runtime.Interfaces;

public interface IAudioBufferManager
{
    int Count { get; }
    AudioBufferContext Current { get; }
    AudioBufferContext Load(int index);
    AudioBufferContext Load(string bufferId);
    bool TryMoveNext(out AudioBufferContext bufferContext);
    bool TryMovePrevious(out AudioBufferContext bufferContext);
    void Reset();
    void Deallocate(string bufferId);
    void DeallocateAll();
}