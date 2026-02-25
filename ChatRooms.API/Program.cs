using ChatRooms.Application;
using ChatRooms.Infrastructure;
using ChatRooms.ServiceDefaults;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var connectionString = builder.Configuration.GetConnectionString("chatroomsdb")
    ?? throw new InvalidOperationException("Connection string 'chatroomsdb' not found.");
builder.AddMongoDBClient("chatrooms-read-db");
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
var app = builder.Build();

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
