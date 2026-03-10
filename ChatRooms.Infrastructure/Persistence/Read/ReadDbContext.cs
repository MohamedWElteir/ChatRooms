using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Application.Users.DTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.Persistence.Read;

public sealed class ReadDbContext(IMongoClient mongoClient)
{
    static ReadDbContext()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonClassMap.RegisterClassMap<RoomDto>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
        BsonClassMap.RegisterClassMap<UserDto>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }
    private readonly IMongoDatabase _database = mongoClient.GetDatabase("chatrooms-read-db");
    public IMongoCollection<RoomDto> Rooms => _database.GetCollection<RoomDto>("Rooms");
    public IMongoCollection<UserDto> Users => _database.GetCollection<UserDto>("Users");

}