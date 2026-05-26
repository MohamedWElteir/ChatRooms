using ChatRooms.Application.Rooms.Queries.GetRoomById;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerGetByIdTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerGetByIdTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object, Mock.Of<Application.Rooms.Queries.IRoomQuery>());
    }

    [Fact]
    public async Task GetById_WhenRoomExists_ShouldReturn200Ok()
    {
        var roomId = Guid.NewGuid();
        var dto = new RoomDto(roomId, "GeneralChat", "ABCD1234", 50, 5, "Active", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetRoomByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetById(roomId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<RoomDto>(okResult.Value);
        Assert.Equal(roomId, returned.Id);
        Assert.Equal("GeneralChat", returned.Name);
    }

    [Fact]
    public async Task GetById_ShouldSendQueryWithCorrectId()
    {
        var roomId = Guid.NewGuid();
        var dto = new RoomDto(roomId, "GeneralChat", "ABCD1234", 50, 5, "Active", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetRoomByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        await _controller.GetById(roomId, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<GetRoomByIdQuery>(q => q.Id == roomId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
