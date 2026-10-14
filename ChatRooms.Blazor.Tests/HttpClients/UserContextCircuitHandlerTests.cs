using System.Security.Claims;
using ChatRooms.Blazor.HttpClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatRooms.Blazor.Tests.HttpClients;

public class UserContextCircuitHandlerTests
{
    private sealed class StubAuthenticationService(AuthenticateResult result) : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(result);

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }

    private static (UserContextCircuitHandler handler, UserContext userContext) CreateHandler(
        HttpContext? httpContext)
    {
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var userContext = new UserContext();
        var handler = new UserContextCircuitHandler(
            accessor,
            userContext,
            NullLogger<UserContextCircuitHandler>.Instance);
        return (handler, userContext);
    }

    private static HttpContext CreateHttpContext(AuthenticateResult authResult)
    {
        var context = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(new StubAuthenticationService(authResult));
        context.RequestServices = services.BuildServiceProvider();
        return context;
    }

    [Fact]
    public async Task OnCircuitOpenedAsync_WhenHttpContextIsNull_MarksSessionExpired()
    {
        var (handler, userContext) = CreateHandler(null);
        userContext.Initialize("sub-old", "token-old", "refresh-old", DateTimeOffset.UtcNow.AddHours(1));

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.Null(userContext.Sub);
        Assert.Null(userContext.AccessToken);
        Assert.Null(userContext.RefreshToken);
        Assert.False(userContext.HasValidAccessToken);
    }

    [Fact]
    public async Task OnCircuitOpenedAsync_WhenAuthenticationFails_MarksSessionExpired()
    {
        var httpContext = CreateHttpContext(AuthenticateResult.Fail("Expired"));
        var (handler, userContext) = CreateHandler(httpContext);
        userContext.Initialize("sub-old", "token-old", "refresh-old", DateTimeOffset.UtcNow.AddHours(1));

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.Null(userContext.Sub);
        Assert.Null(userContext.AccessToken);
        Assert.Null(userContext.RefreshToken);
        Assert.False(userContext.HasValidAccessToken);
    }

    [Fact]
    public async Task OnCircuitOpenedAsync_WhenAuthenticatedWithTokens_InitializesUserContext()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user-42")],
            CookieAuthenticationDefaults.AuthenticationScheme));

        var properties = new AuthenticationProperties();
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = "my-access-token" },
            new AuthenticationToken { Name = "refresh_token", Value = "my-refresh-token" },
            new AuthenticationToken { Name = "expires_at", Value = DateTimeOffset.UtcNow.AddMinutes(15).ToString("o") }
        ]);

        var ticket = new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
        var httpContext = CreateHttpContext(AuthenticateResult.Success(ticket));

        var (handler, userContext) = CreateHandler(httpContext);

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.Equal("user-42", userContext.Sub);
        Assert.Equal("my-access-token", userContext.AccessToken);
        Assert.Equal("my-refresh-token", userContext.RefreshToken);
        Assert.True(userContext.HasValidAccessToken);
    }

    [Fact]
    public async Task OnCircuitOpenedAsync_WhenAuthenticatedWithoutTokens_SetsNullTokens()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user-42")],
            CookieAuthenticationDefaults.AuthenticationScheme));

        var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
        var httpContext = CreateHttpContext(AuthenticateResult.Success(ticket));

        var (handler, userContext) = CreateHandler(httpContext);

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.Equal("user-42", userContext.Sub);
        Assert.Null(userContext.AccessToken);
        Assert.Null(userContext.RefreshToken);
        Assert.False(userContext.HasValidAccessToken);
    }

    private sealed class TestAuthStateProvider(ClaimsPrincipal principal) : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        public override Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(principal));
    }

    [Fact]
    public async Task OnCircuitOpenedAsync_WhenTokensInSessionStore_InitializesEvenIfHttpContextIsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user-store-1")],
            CookieAuthenticationDefaults.AuthenticationScheme));

        var authStateProvider = new TestAuthStateProvider(principal);
        var sessionStore = new ServerSessionStore();
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30);
        sessionStore.Store("user-store-1", "access-from-store", "refresh-from-store", expiry);

        var accessor = new HttpContextAccessor { HttpContext = null };
        var userContext = new UserContext();
        var handler = new UserContextCircuitHandler(
            authStateProvider,
            sessionStore,
            accessor,
            userContext,
            NullLogger<UserContextCircuitHandler>.Instance);

        await handler.OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.Equal("user-store-1", userContext.Sub);
        Assert.Equal("access-from-store", userContext.AccessToken);
        Assert.Equal("refresh-from-store", userContext.RefreshToken);
        Assert.True(userContext.HasValidAccessToken);
    }
}
