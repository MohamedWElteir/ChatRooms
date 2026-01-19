var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api");

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor");

builder.Build().Run();
