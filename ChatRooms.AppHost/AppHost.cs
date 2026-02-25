var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("postgress")
                       .AddDatabase("chatroomsdb");

var mongo = builder.AddMongoDB("mongo")
                    .AddDatabase("chatrooms-read-db");

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api")
        .WithReference(postgress)
        .WithReference(mongo);

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor");

builder.Build().Run();
