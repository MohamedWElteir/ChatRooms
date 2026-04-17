var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                       .WithPgAdmin()
                       .AddDatabase("chatrooms-write-db");

var mongo = builder.AddMongoDB("mongo")
                    .WithMongoExpress()
                    .AddDatabase("chatrooms-read-db");

builder.AddProject<Projects.ChatRooms_API>("chatrooms-api")
        .WithReference(postgres)
        .WaitFor(postgres)
        .WithReference(mongo)
        .WaitFor(mongo);

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor");

builder.Build().Run();
