using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);
var keycloakSecret = builder.AddParameter("keycloak-secret", secret: true);

var keycloak = builder.AddKeycloak(
    "keycloak",
    port: 8080,
    adminPassword: keycloakAdminPassword)
    .WithRealmImport("./KeycloakRealms/");
if (builder.Configuration.GetValue<bool>("PersistKeycloak"))
{
    keycloak.WithDataVolume();
}


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

var blazor = builder.AddProject<Projects.ChatRooms_Blazor>("chatrooms-blazor")
    .WithReference(api)
    .WithReference(bff)
    .WaitFor(bff)
    .WithEnvironment("Keycloak__ClientSecret", keycloakSecret);

bff.WithEnvironment("BlazorAppUrl", blazor.GetEndpoint("https"));

builder.AddProject<Projects.ChatRooms_KeycloakSetup>("chatrooms-keycloak-setup")
    .WithReference(keycloak)
    .WithReference(bff)
    .WithReference(blazor)
    .WaitFor(keycloak)
    .WaitFor(bff)
    .WithEnvironment("Keycloak__AdminUser", "admin")
    .WithEnvironment("Keycloak__AdminPassword", keycloakAdminPassword)
    .WithEnvironment("Keycloak__Realm", "chatrooms")
    .WithEnvironment("Keycloak__BffClientId", "chatrooms-bff");


builder.Build().Run();
