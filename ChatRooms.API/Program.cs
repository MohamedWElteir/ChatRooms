using ChatRooms.Application;
using ChatRooms.Infrastructure;
using ChatRooms.Infrastructure.Persistence.Write;
using ChatRooms.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var connectionString = builder.Configuration.GetConnectionString("chatrooms-write-db")
    ?? throw new InvalidOperationException("Connection string 'chatrooms-write-db' not found.");
builder.AddMongoDBClient("chatrooms-read-db");
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
