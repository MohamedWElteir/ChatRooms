using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Application.Rooms.Queries.GetRoomByCode;
using ChatRooms.DTOs.Rooms;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class GetRoomByCodeQueryHandlerTests
{
    private readonly Mock<IRoomQuery> _roomQueryMock;
    private readonly GetRoomByCodeQueryHandler _handler;

    public GetRoomByCodeQueryHandlerTests()
    {
        _roomQueryMock = new Mock<IRoomQuery>();
        _handler = new GetRoomByCodeQueryHandler(_roomQueryMock.Object);
    }

    [Fact]
    public async Task Handle_RoomFound_ShouldReturnDto()
    {
        var roomId = Guid.NewGuid();
        var dto = new RoomDto(roomId, "TestRoom", "VALID123", 50, 0, "Active", 1);
        _roomQueryMock.Setup(x => x.GetByCodeAsync("VALID123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var command = new GetRoomByCodeQuery("VALID123");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(roomId, result.Id);
        Assert.Equal("TestRoom", result.Name);
        Assert.Equal("VALID123", result.Code);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldThrowKeyNotFoundException()
    {
        _roomQueryMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomDto?)null);

        var command = new GetRoomByCodeQuery("NOTFOUND");

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
