var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("postgress")
                       .AddDatabase("chatroomsdb");

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api")
        .WithReference(postgress);

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor");

builder.Build().Run();
