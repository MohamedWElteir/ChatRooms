using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Users.Commands;
using ChatRooms.Application.Users.Commands.CreateUser;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Domain.Users.ValueObjects;
using ChatRooms.DTOs.Users;
using Moq;

namespace ChatRooms.Application.Tests.Users;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));

        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUserAndReturnDto()
    {
        var command = new CreateUserCommand("JohnDoe", "john@test.com", Gender.Male, new DateTime(2000, 1, 1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("JohnDoe", result.Value.Name);
        Assert.Equal("john@test.com", result.Value.Email);
        Assert.Equal("Male", result.Value.Gender);

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNameThrows_ShouldFail()
    {
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));

        var handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateUserCommand("", "john@test.com", Gender.Male, new DateTime(2000, 1, 1)), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEmailInvalid_ShouldFail()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new CreateUserCommand("JohnDoe", "not-an-email", Gender.Male, new DateTime(2000, 1, 1)), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenBirthDateInFuture_ShouldFail()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new CreateUserCommand("JohnDoe", "john@test.com", Gender.Male, new DateTime(2030, 1, 1)), CancellationToken.None));
    }
}
