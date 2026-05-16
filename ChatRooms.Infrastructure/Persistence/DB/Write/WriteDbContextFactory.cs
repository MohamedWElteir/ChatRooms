using ChatRooms.Infrastructure.Persistence.Outbox;
using ChatRooms.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChatRooms.Infrastructure.Persistence.DB.Write;

public sealed class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ChatRooms.API"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("postgress")
            ?? throw new InvalidOperationException("Connection string 'postgress' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        var jsonOptions = JsonOptionsFactory.Create();
        var factory = new OutboxMessageFactory(jsonOptions);
        return new WriteDbContext(optionsBuilder.Options, factory);
    }
}