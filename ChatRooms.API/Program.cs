using ChatRooms.Application;
using ChatRooms.Infrastructure;
using ChatRooms.ServiceDefaults;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>();
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
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
