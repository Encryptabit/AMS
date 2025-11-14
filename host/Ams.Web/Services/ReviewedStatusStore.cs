using System.Text.Json;
using Ams.Web.Configuration;
using Ams.Web.Utilities;
using Microsoft.Extensions.Options;

namespace Ams.Web.Services;

public sealed class ReviewedStatusStore
{
    private readonly string _storePath;
    private readonly SemaphoreSlim _ioGate = new(1, 1);
    private Dictionary<string, bool> _state = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public ReviewedStatusStore(IOptions<AmsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var fileName = options.Value.ReviewedStatus.StoreFileName;
        _storePath = AppDataPaths.Resolve(string.IsNullOrWhiteSpace(fileName) ? "reviewed-status.json" : fileName);
        var directory = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<bool> GetAsync(string chapterId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        await EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
        lock (_state)
        {
            return _state.TryGetValue(chapterId, out var reviewed) && reviewed;
        }
    }

    public async Task SetAsync(string chapterId, bool reviewed, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        await EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
        lock (_state)
        {
            _state[chapterId] = reviewed;
        }

        await PersistAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_loaded)
        {
            return;
        }

        await _ioGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_loaded)
            {
                return;
            }

            if (File.Exists(_storePath))
            {
                await using var stream = File.OpenRead(_storePath);
                var payload = await JsonSerializer.DeserializeAsync<Dictionary<string, bool>>(stream, cancellationToken: cancellationToken)
                               .ConfigureAwait(false);
                if (payload is not null)
                {
                    _state = new Dictionary<string, bool>(payload, StringComparer.OrdinalIgnoreCase);
                }
            }

            _loaded = true;
        }
        finally
        {
            _ioGate.Release();
        }
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        await _ioGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var stream = File.Create(_storePath);
            await JsonSerializer.SerializeAsync(
                stream,
                _state,
                options: new JsonSerializerOptions { WriteIndented = true },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ioGate.Release();
        }
    }
}
