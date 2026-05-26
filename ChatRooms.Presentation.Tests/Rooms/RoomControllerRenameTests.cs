using ChatRooms.Application.Rooms.Commands.RenameRoom;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.Presentation.Rooms;
using ChatRooms.Presentation.Rooms.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerRenameTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerRenameTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object, Mock.Of<Application.Rooms.Queries.IRoomQuery>());
    }

    [Fact]
    public async Task Rename_WhenSuccess_ShouldReturn200OkWithNewName()
    {
        var roomId = Guid.NewGuid();
        var request = new RenameRoomRequest("NewName");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RenameRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<string>)"NewName");

        var result = await _controller.Rename(roomId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("NewName", okResult.Value);
    }

    [Fact]
    public async Task Rename_WhenRoomNotFound_ShouldReturn404()
    {
        var request = new RenameRoomRequest("NewName");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RenameRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<string>)RoomErrors.NotFound);

        var result = await _controller.Rename(Guid.NewGuid(), request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Room.NotFound", problem.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task Rename_WhenRoomNotActive_ShouldReturn400()
    {
        var request = new RenameRoomRequest("NewName");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RenameRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<string>)RoomErrors.NotActive);

        var result = await _controller.Rename(Guid.NewGuid(), request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task Rename_ShouldSendExpectedCommand()
    {
        var roomId = Guid.NewGuid();
        var request = new RenameRoomRequest("UpdatedName");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RenameRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<string>)"UpdatedName");

        await _controller.Rename(roomId, request, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<RenameRoomCommand>(c =>
                c.Id == RoomId.From(roomId) &&
                c.NewName == "UpdatedName"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
