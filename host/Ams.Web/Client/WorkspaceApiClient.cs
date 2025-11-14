using System.Net.Http.Json;
using Ams.Web.Dtos;
using Ams.Web.Requests;

namespace Ams.Web.Client;

public sealed class WorkspaceApiClient
{
    private readonly HttpClient _httpClient;

    public WorkspaceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WorkspaceStateDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<WorkspaceStateDto>(
            "api/workspace",
            cancellationToken);

        return result ?? throw new InvalidOperationException("Workspace state was not returned by the server.");
    }

    public async Task<WorkspaceStateDto> UpdateAsync(WorkspaceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/workspace", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<WorkspaceStateDto>(cancellationToken: cancellationToken)
                      .ConfigureAwait(false);

        return payload ?? throw new InvalidOperationException("Failed to deserialize workspace response.");
    }
}
