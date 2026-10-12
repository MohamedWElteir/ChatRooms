using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Application.Rooms.Queries.GetAllRooms;
using ChatRooms.DTOs.Rooms;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class GetAllRoomsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRoomsFromQuery()
    {
        var queryMock = new Mock<IRoomQuery>();
        var rooms = new List<RoomListItem>
        {
            new(Guid.NewGuid(), "Room1", "CODE1", 10, 2),
            new(Guid.NewGuid(), "Room2", "CODE2", 20, 5)
        };

        queryMock.Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms);

        var handler = new GetAllRoomsQueryHandler(queryMock.Object);

        var result = await handler.Handle(new GetAllRoomsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        queryMock.Verify(q => q.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
