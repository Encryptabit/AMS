namespace Ams.Core.Services.Interfaces;

public interface IAudioService
{
    Task WarmAsync(CancellationToken cancellationToken = default);
}