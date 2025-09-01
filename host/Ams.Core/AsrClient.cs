using System.Text;
using System.Text.Json;

namespace Ams.Core;

public class AsrClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private bool _disposed;

    public AsrClient(string baseUrl = "http://localhost:8000")
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
    }

    public async Task<AsrResponse> TranscribeAsync(string audioPath, string? model = null, string language = "en", CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}", audioPath);

        var request = new AsrRequest(
            AudioPath: Path.GetFullPath(audioPath),
            Model: model,
            Language: language
        );

        var json = JsonSerializer.Serialize(request, JsonSerializerOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/asr", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"ASR service returned {response.StatusCode}: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<AsrResponse>(responseJson);
            
            return result ?? throw new InvalidOperationException("Failed to deserialize ASR response");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException("ASR service request timed out", ex);
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}