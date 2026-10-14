using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ChatRooms.Blazor.HttpClients;

/// <summary>
/// Publishes the circuit's scoped service provider to <see cref="CircuitServicesAccessor"/>
/// for the duration of every inbound circuit activity (renders, event handlers, JS interop).
/// </summary>
public sealed class ServicesAccessorCircuitHandler(
    IServiceProvider services,
    CircuitServicesAccessor servicesAccessor) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
        => async context =>
        {
            servicesAccessor.Services = services;
            await next(context);
            servicesAccessor.Services = null;
        };
}
