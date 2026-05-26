using ChatRooms.Application.Rooms.Queries;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerGetAllTests
{
    private readonly Mock<IRoomQuery> _queryMock;
    private readonly RoomController _controller;

    public RoomControllerGetAllTests()
    {
        _queryMock = new Mock<IRoomQuery>();
        _controller = new RoomController(Mock.Of<ISender>(), _queryMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturn200OkWithRooms()
    {
        var rooms = new List<RoomListItem>
        {
            new(Guid.NewGuid(), "GeneralChat", "ABCD1234", 50, 5),
            new(Guid.NewGuid(), "TechTalk", "XYZ78901", 100, 42)
        };

        _queryMock
            .Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<RoomListItem>)rooms);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRooms = Assert.IsAssignableFrom<IReadOnlyList<RoomListItem>>(okResult.Value);
        Assert.Equal(2, returnedRooms.Count);
    }

    [Fact]
    public async Task GetAll_WhenNoRooms_ShouldReturn200OkWithEmptyList()
    {
        _queryMock
            .Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<RoomListItem>)[]);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRooms = Assert.IsAssignableFrom<IReadOnlyList<RoomListItem>>(okResult.Value);
        Assert.Empty(returnedRooms);
    }

    [Fact]
    public async Task GetAll_ShouldCallQueryService()
    {
        _queryMock
            .Setup(q => q.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<RoomListItem>)[]);

        await _controller.GetAll(CancellationToken.None);

        _queryMock.Verify(q => q.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
