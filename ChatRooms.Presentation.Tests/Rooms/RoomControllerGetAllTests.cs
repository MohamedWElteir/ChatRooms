using ChatRooms.Application.Rooms.Queries.GetAllRooms;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerGetAllTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerGetAllTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturn200OkWithRooms()
    {
        var rooms = new List<RoomListItem>
        {
            new(Guid.NewGuid(), "GeneralChat", "ABCD1234", 50, 5),
            new(Guid.NewGuid(), "TechTalk", "XYZ78901", 100, 42)
        };

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAllRoomsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRooms = Assert.IsAssignableFrom<IReadOnlyList<RoomListItem>>(okResult.Value);
        Assert.Equal(2, returnedRooms.Count);
    }

    [Fact]
    public async Task GetAll_WhenNoRooms_ShouldReturn200OkWithEmptyList()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAllRoomsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomListItem>());

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRooms = Assert.IsAssignableFrom<IReadOnlyList<RoomListItem>>(okResult.Value);
        Assert.Empty(returnedRooms);
    }

    [Fact]
    public async Task GetAll_ShouldSendQuery()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetAllRoomsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoomListItem>());

        await _controller.GetAll(CancellationToken.None);

        _senderMock.Verify(s => s.Send(It.IsAny<GetAllRoomsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
