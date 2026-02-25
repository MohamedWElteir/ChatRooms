using ChatRooms.Application.Rooms.DTOs;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.Persistence.Read;

public sealed class ReadDbContext(IMongoClient mongoClient)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase("chatrooms-read-db");
    public IMongoCollection<RoomDto> Rooms => _database.GetCollection<RoomDto>("Rooms");

}