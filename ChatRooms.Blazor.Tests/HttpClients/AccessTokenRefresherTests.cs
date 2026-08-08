using System.Net;
using ChatRooms.Blazor.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatRooms.Blazor.Tests.HttpClients;

public class AccessTokenRefresherTests
{
    private static AccessTokenRefresher CreateRefresher(
        StubHttpMessageHandler handler, UserContext userContext)
    {
        return new AccessTokenRefresher(
            new StubHttpClientFactory(handler),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Keycloak:Realm"] = "chatrooms",
                    ["Keycloak:ClientId"] = "chatrooms-bff",
                    ["Keycloak:ClientSecret"] = "test-secret",
                    ["Keycloak:Authority"] = "https://keycloak.local/realms/chatrooms"
                })
                .Build(),
            NullLogger<AccessTokenRefresher>.Instance);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_WhenTokenValid_DoesNotRefresh()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Failure()));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "still-valid",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(5));

        var refresher = CreateRefresher(handler, userContext);

        var token = await refresher.GetValidAccessTokenAsync(
            userContext, CancellationToken.None);

        Assert.Equal("still-valid", token);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_WhenExpired_RefreshesExactlyOnce()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Success("new-access", "new-refresh")));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var token = await refresher.GetValidAccessTokenAsync(
            userContext, CancellationToken.None);

        Assert.Equal("new-access", token);
        Assert.Equal(1, handler.RequestCount);
        Assert.Equal("new-access", userContext.AccessToken);
        Assert.Equal("new-refresh", userContext.RefreshToken);
        Assert.True(userContext.AccessTokenExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ConcurrentCallers_SingleFlight()
    {
        var handler = new StubHttpMessageHandler(async _ =>
        {
            await Task.Delay(100);
            return KeycloakResponses.Success("fresh-token");
        });

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => refresher.GetValidAccessTokenAsync(userContext, CancellationToken.None))
            .ToArray();

        var tokens = await Task.WhenAll(tasks);

        Assert.All(tokens, t => Assert.Equal("fresh-token", t));
        Assert.Equal(1, handler.RequestCount);
        Assert.Equal("fresh-token", userContext.AccessToken);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ConcurrentWaiters_ReuseRefreshedToken()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Success("fresh-token")));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var tokens = await Task.WhenAll(
            refresher.GetValidAccessTokenAsync(userContext, CancellationToken.None),
            refresher.GetValidAccessTokenAsync(userContext, CancellationToken.None));

        Assert.All(tokens, t => Assert.Equal("fresh-token", t));
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_RefreshFails_ReturnsNull_AndLeavesSessionIntact()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Failure()));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var token = await refresher.GetValidAccessTokenAsync(
            userContext, CancellationToken.None);

        Assert.Null(token);
        Assert.Equal("refresh-1", userContext.RefreshToken);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_NoRefreshToken_ReturnsNull_WithoutHttpCall()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Success("fresh-token")));

        var userContext = new UserContext();
        userContext.Initialize("sub-1", "expired-token", null, DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var token = await refresher.GetValidAccessTokenAsync(
            userContext, CancellationToken.None);

        Assert.Null(token);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public void UserContext_Initialize_ThenMarkSessionExpired_ClearsTokens()
    {
        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddMinutes(5));

        userContext.MarkSessionExpired();

        Assert.Null(userContext.Sub);
        Assert.Null(userContext.AccessToken);
        Assert.Null(userContext.RefreshToken);
        Assert.False(userContext.HasValidAccessToken);
    }

    [Fact]
    public async Task AuthorizationDelegatingHandler_AttachesBearerToken_WhenTokenRefreshed()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Success("fresh-token")));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var captured = Array.Empty<byte>();
        var inner = new StubHttpMessageHandler(request =>
        {
            captured = request.Headers.Authorization?.Parameter is null
                ? []
                : System.Text.Encoding.UTF8.GetBytes(request.Headers.Authorization.Parameter);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var delegating = new AuthorizationDelegatingHandler(userContext, refresher)
        {
            InnerHandler = inner
        };

        var client = new HttpClient(delegating);
        var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "http://api.local/rooms"));

        Assert.Equal(System.Text.Encoding.UTF8.GetBytes("fresh-token"), captured);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task AuthorizationDelegatingHandler_RefreshFails_ThrowsUnauthorized()
    {
        var handler = new StubHttpMessageHandler(
            _ => Task.FromResult(KeycloakResponses.Failure()));

        var userContext = new UserContext();
        userContext.Initialize(
            "sub-1",
            "expired-token",
            "refresh-1",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        var refresher = CreateRefresher(handler, userContext);

        var delegating = new AuthorizationDelegatingHandler(userContext, refresher)
        {
            InnerHandler = new StubHttpMessageHandler(
                _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
        };

        var client = new HttpClient(delegating);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://api.local/rooms")));

        Assert.False(userContext.HasValidAccessToken);
        Assert.Null(userContext.RefreshToken);
        Assert.Null(userContext.Sub);
    }
}