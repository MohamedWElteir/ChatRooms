using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithRealmImport("./KeycloakRealms/");
if (builder.Configuration.GetValue<bool>("PersistKeycloak"))
{
    keycloak.WithDataVolume();
}
var keycloakSecret = builder.AddParameter("keycloak-secret", secret: true);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("chatrooms-write-db");

var mongo = builder.AddMongoDB("mongo")
    .WithMongoExpress()
    .AddDatabase("chatrooms-read-db");

var api = builder.AddProject<Projects.ChatRooms_API>("chatrooms-api")
    .WithReference(postgres)
    .WithReference(mongo)
    .WithReference(keycloak)
    .WaitFor(postgres)
    .WaitFor(mongo)
    .WaitFor(keycloak);

var bff = builder.AddProject<Projects.ChatRooms_BFF>("chatrooms-bff")
    .WithReference(api)
    .WithReference(keycloak)
    .WaitFor(api)
    .WaitFor(keycloak)
    .WithEnvironment("Keycloak__ClientSecret", keycloakSecret);

builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor")
    .WithReference(bff);

builder.AddProject<Projects.ChatRooms_KeycloakSetup>("chatrooms-keycloak-setup")
    .WithReference(keycloak)
    .WithReference(bff)
    .WaitFor(keycloak)
    .WaitFor(bff)
    .WithEnvironment("Keycloak__AdminUser", "admin")
    .WithEnvironment("Keycloak__AdminPassword", "admin")
    .WithEnvironment("Keycloak__Realm", "chatrooms")
    .WithEnvironment("Keycloak__BffClientId", "chatrooms-bff")
    .WithEnvironment("Keycloak__ClientSecret", keycloakSecret);


builder.Build().Run();
