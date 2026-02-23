using Ams.Core.Artifacts;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Singleton that holds a transient in-memory preview buffer for the Polish workflow.
/// PolishService is transient and cannot hold preview state itself — this singleton
/// bridges the Blazor component and AudioController.
/// </summary>
public sealed class PreviewBufferService
{
    private readonly object _lock = new();

    public AudioBuffer? Buffer { get; private set; }
    public long Version { get; private set; }

    public void Set(AudioBuffer buffer)
    {
        lock (_lock)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            Version++;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            Buffer = null;
        }
    }
}
