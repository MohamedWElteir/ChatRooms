using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatRooms.BFF.Services;

namespace ChatRooms.BFF.HttpClients;

public sealed class InternalApiClient(
    HttpClient httpClient,
    KeycloakTokenService tokenService)
{
    private async Task AttachServiceTokenAsync(CancellationToken ct)
    {
        var token = await tokenService.GetServiceAccountTokenAsync(ct);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(
        string path, T payload, CancellationToken ct)
    {
        await AttachServiceTokenAsync(ct);
        return await httpClient.PostAsJsonAsync(path, payload, ct);
    }

    public async Task<HttpResponseMessage> GetAsync(
        string path, CancellationToken ct)
    {
        await AttachServiceTokenAsync(ct);
        return await httpClient.GetAsync(path, ct);
    }
}
