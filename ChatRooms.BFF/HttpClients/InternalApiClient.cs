using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatRooms.BFF.Services;

namespace ChatRooms.BFF.HttpClients;

public sealed class InternalApiClient(
    HttpClient httpClient,
    KeycloakTokenService tokenService)
{
    private async Task<AuthenticationHeaderValue> CreateAuthorizationHeaderAsync(CancellationToken ct)
    {
        var token = await tokenService.GetServiceAccountTokenAsync(ct);
        return new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(
        string path, T payload, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = await CreateAuthorizationHeaderAsync(ct);
        return await httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> GetAsync(
        string path, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = await CreateAuthorizationHeaderAsync(ct);
        return await httpClient.SendAsync(request, ct);
    }
}
