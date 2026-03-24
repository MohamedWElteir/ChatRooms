using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.Queries;
using ChatRooms.Infrastructure.Persistence.Repositories;
using ChatRooms.Infrastructure.Security;
using ChatRooms.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using ChatRooms.Infrastructure.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using ChatRooms.Infrastructure.Persistence.DB.Read;

namespace ChatRooms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("chatrooms-write-db")
        ?? throw new InvalidOperationException("Connection string 'chatrooms-write-db' not found.");

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddSingleton<JsonSerializerOptions>(JsonOptionsFactory.Create());
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