using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Infrastructure.Security;
using ChatRooms.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRooms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}