using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Persistence.Queries;
using ChatRooms.Infrastructure.Persistence.Read;
using ChatRooms.Infrastructure.Persistence.Repositories;
using ChatRooms.Infrastructure.Persistence.Write;
using ChatRooms.Infrastructure.Security;
using ChatRooms.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRooms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WriteDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddSingleton<ReadDbContext>();
        services.AddScoped<IRoomQuery, RoomQueryService>();
        services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddKeyedScoped<IEventProjector, RoomCreatedProjector>(nameof(RoomCreatedDomainEvent));
        services.AddHostedService<OutboxProcessor>();
        services.AddSingleton<IRoomCapacityPolicy, DefaultRoomCapacityPolicy>();
        return services;
    }
}