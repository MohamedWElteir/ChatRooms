using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Users.Commands;
using ChatRooms.Application.Users.Commands.DeleteUser;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;
using Moq;

namespace ChatRooms.Application.Tests.Users;

public sealed class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly DeleteUserCommandHandler _handler;
    private readonly User _existingUser;
    private readonly Guid _userId;

    public DeleteUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));

        var createResult = User.Create(
            ChatRooms.Domain.Users.ValueObjects.Name.From("TestUser"),
            Email.From("test@test.com"),
            Domain.Users.Enums.Gender.Male,
            BirthDate.From(new DateTime(2000, 1, 1)),
            DateTimeUtc.FromUtc(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc)));
        _existingUser = createResult.Value!;
        _userId = _existingUser.Id;

        _userRepositoryMock.Setup(x => x.GetById(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingUser);

        _handler = new DeleteUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldSucceed()
    {
        var command = new DeleteUserCommand(_userId, DeletionReason.DeletedByUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundError()
    {
        _userRepositoryMock.Setup(x => x.GetById(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new DeleteUserCommand(Guid.NewGuid(), DeletionReason.DeletedByUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AlreadyDeleted_ShouldFail()
    {
        _existingUser.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new DeleteUserCommand(_userId, DeletionReason.DeletedByUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
