using ChatRooms.Blazor.HttpClients;
using ChatRooms.Blazor.Services;

namespace ChatRooms.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        services.AddHttpClient<IRoomApiClient, RoomApiClient>(client =>
        {
            client.BaseAddress = new Uri("https+http://chatrooms-api");
        })
        .AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        services.AddHttpClient("bff", client =>
        {
            client.BaseAddress = new Uri("https+http://chatrooms-bff");
        });

        services.AddScoped<ToastService>();
        return services;
    }
}
