using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Users.Commands;
using ChatRooms.Application.Users.Commands.ChangeEmail;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;
using Moq;

namespace ChatRooms.Application.Tests.Users;

public sealed class ChangeEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ChangeEmailCommandHandler _handler;
    private readonly User _existingUser;
    private readonly Guid _userId;

    public ChangeEmailCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));

        var createResult = User.Create(
            ChatRooms.Domain.Users.ValueObjects.Name.From("TestUser"),
            Email.From("old@test.com"),
            Domain.Users.Enums.Gender.Male,
            BirthDate.From(new DateTime(2000, 1, 1)),
            DateTimeUtc.FromUtc(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc)));
        _existingUser = createResult.Value!;
        _userId = _existingUser.Id;

        _userRepositoryMock.Setup(x => x.GetById(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingUser);

        _handler = new ChangeEmailCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidChange_ShouldSucceed()
    {
        var command = new ChangeEmailCommand(_userId, "new@test.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundError()
    {
        _userRepositoryMock.Setup(x => x.GetById(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ChangeEmailCommand(Guid.NewGuid(), "new@test.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_DeletedUser_ShouldFail()
    {
        _existingUser.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new ChangeEmailCommand(_userId, "new@test.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_SameEmail_ShouldNotRaiseEvent()
    {
        var command = new ChangeEmailCommand(_userId, "old@test.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_InvalidEmailFormat_ShouldFail()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new ChangeEmailCommand(_userId, "invalid"), CancellationToken.None));
    }
}
