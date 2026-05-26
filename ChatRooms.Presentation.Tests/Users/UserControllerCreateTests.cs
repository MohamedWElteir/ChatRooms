using ChatRooms.Application.Users.Commands.CreateUser;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.DTOs.Users;
using ChatRooms.Presentation.Users;
using ChatRooms.Presentation.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ChatRooms.Presentation.Tests.Users;

public sealed class UserControllerCreateTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly UserController _controller;

    public UserControllerCreateTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new UserController(_senderMock.Object);
    }

    [Fact]
    public async Task Create_WhenSuccess_ShouldReturn201Created()
    {
        var userId = Guid.NewGuid();
        var dto = new UserDto(userId, "JohnDoe", "john@test.com", "Male", 1);
        var request = new CreateUserRequest("JohnDoe", "john@test.com", Gender.Male, new DateTime(2000, 1, 1));

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<UserDto>)dto);

        var result = await _controller.Create(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal("GetById", createdResult.ActionName);
        Assert.Equal(userId, createdResult.RouteValues?["id"]);
        Assert.Same(dto, createdResult.Value);
    }

    [Fact]
    public async Task Create_WhenCommandFails_ShouldReturnProblemDetails()
    {
        var error = new Error("User.NotTransient", "Only transient users can be created.");
        var request = new CreateUserRequest("JohnDoe", "john@test.com", Gender.Male, new DateTime(2000, 1, 1));

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<UserDto>)error);

        var result = await _controller.Create(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("User.NotTransient", problem.Title);
        Assert.Equal("Only transient users can be created.", problem.Detail);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public async Task Create_ShouldSendExpectedCommand()
    {
        var request = new CreateUserRequest("JaneDoe", "jane@test.com", Gender.Female, new DateTime(1995, 5, 10));
        var dto = new UserDto(Guid.NewGuid(), "JaneDoe", "jane@test.com", "Female", 1);

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<UserDto>)dto);

        await _controller.Create(request, CancellationToken.None);

        _senderMock.Verify(s => s.Send(
            It.Is<CreateUserCommand>(c =>
                c.Name == "JaneDoe" &&
                c.Email == "jane@test.com" &&
                c.Gender == Gender.Female &&
                c.BirthDate == new DateTime(1995, 5, 10)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
