using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Services;

/// <summary>
/// Coordinates FFmpeg-backed audio workflows. Implementation will be provided alongside processor work.
/// </summary>
public sealed class AudioService
{
    public AudioService()
    {
    }

    public Task WarmAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
