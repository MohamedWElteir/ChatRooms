using ChatRooms.Application.Rooms.Queries.GetRoomByCode;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerGetByCodeTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerGetByCodeTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object, Mock.Of<Application.Rooms.Queries.IRoomQuery>());
    }

    [Fact]
    public async Task GetByCode_WhenRoomExists_ShouldReturn200Ok()
    {
        var dto = new RoomDto(Guid.NewGuid(), "GeneralChat", "ABCD1234", 50, 5, "Active", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetRoomByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetByCode("ABCD1234", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<RoomDto>(okResult.Value);
        Assert.Equal("ABCD1234", returned.Code);
    }

    [Fact]
    public async Task GetByCode_ShouldSendQueryWithCorrectCode()
    {
        var dto = new RoomDto(Guid.NewGuid(), "GeneralChat", "ABCD1234", 50, 5, "Active", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetRoomByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        await _controller.GetByCode("ABCD1234", CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<GetRoomByCodeQuery>(q => q.Code == "ABCD1234"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
