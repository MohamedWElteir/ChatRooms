var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api");

builder.Build().Run();
