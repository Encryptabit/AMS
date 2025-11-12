namespace Ams.Core.Services;

public interface IAudioService
{
    Task WarmAsync(CancellationToken cancellationToken = default);
}