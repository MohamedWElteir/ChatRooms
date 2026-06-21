using ChatRooms.Application.Rooms.Commands.DeleteRoom;
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

public sealed class RoomControllerDeleteTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerDeleteTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object, Mock.Of<Application.Rooms.Queries.IRoomQuery>());
    }

    [Fact]
    public async Task Delete_WhenSuccess_ShouldReturn204NoContent()
    {
        var request = new DeleteRoomRequest("No longer needed");

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid(), request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenRoomNotFound_ShouldReturn404()
    {
        var request = new DeleteRoomRequest("No longer needed");

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result)RoomErrors.NotFound);

        var result = await _controller.Delete(Guid.NewGuid(), request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Room.NotFound", problem.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task Delete_WhenRoomIsActive_ShouldReturn400()
    {
        var request = new DeleteRoomRequest("Inactivity");

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result)RoomErrors.ActiveRoomCannotBeDeletedDueToInactivity);

        var result = await _controller.Delete(Guid.NewGuid(), request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldSendExpectedCommand()
    {
        var roomId = Guid.NewGuid();
        var request = new DeleteRoomRequest("Archived, deleting");

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        await _controller.Delete(roomId, request, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<DeleteRoomCommand>(c =>
                c.RoomId == RoomId.From(roomId) &&
                c.DeletionReason == "Archived, deleting"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
