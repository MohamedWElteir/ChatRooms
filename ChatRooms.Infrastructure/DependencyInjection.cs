using ChatRooms.Application.Abstractions.Common;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Policies.Room;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Application.Users.Commands;
using ChatRooms.Application.Users.Queries;
using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using ChatRooms.Infrastructure.Persistence.Outbox;
using ChatRooms.Infrastructure.Persistence.Queries;
using ChatRooms.Infrastructure.Persistence.Repositories;
using ChatRooms.Infrastructure.Security;
using ChatRooms.Infrastructure.Serialization;
using ChatRooms.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ChatRooms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("chatrooms-write-db")
        ?? throw new InvalidOperationException("Connection string 'chatrooms-write-db' not found.");

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddSingleton(JsonOptionsFactory.Create());
        services.AddSingleton<IOutboxMessageFactory, OutboxMessageFactory>();
        services.AddSingleton<IOutboxMessageProcessor, OutboxMessageProcessor>();
        services.AddDbContext<WriteDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ReadDbContext>();
        services.AddScoped<IRoomQuery, RoomQueryService>();
        services.AddScoped<IUserQuery, UserQueryService>();
        services.AddSingleton<IGenerator<RoomCode>, RoomCodeGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddKeyedScoped<IEventProjector, RoomCreatedProjector>(nameof(RoomCreatedDomainEvent));
        services.AddKeyedScoped<IEventProjector, RoomRenamedProjector>(nameof(RoomRenamedDomainEvent));
        services.AddKeyedScoped<IEventProjector, UserCreatedProjector>(nameof(UserCreatedDomainEvent));
        services.AddHostedService<OutboxProcessor>();
        services.AddSingleton<IRoomCapacityPolicy, DefaultRoomCapacityPolicy>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Keycloak:Authority"];
                options.Audience = "chatrooms-api";
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("BffServiceOnly", policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("azp", "chatrooms-bff"));

        return services;
    }
}