using ChatRooms.Application.Rooms.Commands.RestoreRoom;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.Presentation.Rooms;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerRestoreTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerRestoreTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object);
    }

    [Fact]
    public async Task Restore_WhenSuccess_ShouldReturn204NoContent()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RestoreRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Restore(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Restore_WhenRoomNotFound_ShouldReturn404()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RestoreRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result)RoomErrors.NotFound);

        var result = await _controller.Restore(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Room.NotFound", problem.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task Restore_WhenNotArchived_ShouldReturn400()
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<RestoreRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result)RoomErrors.OnlyArchivedCanBeRestored);

        var result = await _controller.Restore(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task Restore_ShouldSendExpectedCommand()
    {
        var roomId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.IsAny<RestoreRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        await _controller.Restore(roomId, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<RestoreRoomCommand>(c => c.RoomId == RoomId.From(roomId)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
