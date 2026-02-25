var builder = DistributedApplication.CreateBuilder(args);

var postgress = builder.AddPostgres("postgress")
                       .WithPgAdmin()
                       .AddDatabase("chatrooms-write-db");

var mongo = builder.AddMongoDB("mongo")
                    .WithMongoExpress()
                    .AddDatabase("chatrooms-read-db");

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api")
        .WithReference(postgress)
        .WaitFor(postgress)
        .WithReference(mongo)
        .WaitFor(mongo);

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor");

builder.Build().Run();
