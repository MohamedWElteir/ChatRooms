using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Application.Rooms.Queries.GetRoomById;
using ChatRooms.DTOs.Rooms;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class GetRoomByIdQueryHandlerTests
{
    private readonly Mock<IRoomQuery> _roomQueryMock;
    private readonly GetRoomByIdQueryHandler _handler;

    public GetRoomByIdQueryHandlerTests()
    {
        _roomQueryMock = new Mock<IRoomQuery>();
        _handler = new GetRoomByIdQueryHandler(_roomQueryMock.Object);
    }

    [Fact]
    public async Task Handle_RoomFound_ShouldReturnDto()
    {
        var roomId = Guid.NewGuid();
        var dto = new RoomDto(roomId, "TestRoom", "VALID123", 50, 0, "Active", 1);
        _roomQueryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var command = new GetRoomByIdQuery(roomId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(roomId, result.Id);
        Assert.Equal("TestRoom", result.Name);
        Assert.Equal("VALID123", result.Code);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldThrowKeyNotFoundException()
    {
        _roomQueryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomDto?)null);

        var command = new GetRoomByIdQuery(Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
