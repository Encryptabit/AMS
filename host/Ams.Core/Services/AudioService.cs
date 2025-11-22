using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services;

/// <summary>
/// Coordinates FFmpeg-backed audio workflows. Implementation will be provided alongside processor work.
/// </summary>
public sealed class AudioService : IAudioService
{
    public AudioService()
    {
    }

    public Task WarmAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}