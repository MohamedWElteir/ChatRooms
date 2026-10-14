namespace ChatRooms.Blazor.HttpClients;

/// <summary>
/// Exposes the current Blazor circuit's scoped <see cref="IServiceProvider"/> to code that
/// runs in a different DI scope. <see cref="IHttpClientFactory"/> builds message handlers in
/// its own scope, so a <see cref="DelegatingHandler"/> that injects circuit-scoped services
/// (e.g. <see cref="UserContext"/>) directly would receive fresh, empty instances.
/// See: https://learn.microsoft.com/aspnet/core/blazor/fundamentals/dependency-injection#access-server-side-blazor-services-from-a-different-di-scope
/// </summary>
public sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> BlazorServices = new();

    public IServiceProvider? Services
    {
        get => BlazorServices.Value;
        set => BlazorServices.Value = value;
    }
}
