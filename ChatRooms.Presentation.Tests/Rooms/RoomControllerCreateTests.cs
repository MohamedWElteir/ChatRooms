using ChatRooms.Application.Rooms.Commands.CreateRoom;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms;
using ChatRooms.Presentation.Rooms.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Rooms;

public sealed class RoomControllerCreateTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly RoomController _controller;

    public RoomControllerCreateTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new RoomController(_senderMock.Object, Mock.Of<Application.Rooms.Queries.IRoomQuery>());
    }

    [Fact]
    public async Task Create_WhenSuccess_ShouldReturn201Created()
    {
        var roomId = RoomId.New();
        var dto = new RoomDto(roomId, "GeneralChat", "ABCD1234", 50, 0, "Active", 1);
        var request = new CreateRoomRequest("GeneralChat", 50);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<RoomDto>)dto);

        var result = await _controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal("GetById", createdResult.ActionName);
        Assert.Equal(roomId.Value, createdResult.RouteValues?["id"]);
        Assert.Same(dto, createdResult.Value);
    }

    [Fact]
    public async Task Create_WhenCommandFails_ShouldReturnProblemDetails()
    {
        var request = new CreateRoomRequest("GeneralChat", 50);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<RoomDto>)RoomErrors.NotTransient);

        var result = await _controller.Create(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Room.NotTransient", problem.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public async Task Create_ShouldSendExpectedCommand()
    {
        var request = new CreateRoomRequest("TechTalk", 100);
        var dto = new RoomDto(Guid.NewGuid(), "TechTalk", "XYZ78901", 100, 0, "Active", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateRoomCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<RoomDto>)dto);

        await _controller.Create(request, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<CreateRoomCommand>(c =>
                c.Name == "TechTalk" &&
                c.Capacity == 100),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
