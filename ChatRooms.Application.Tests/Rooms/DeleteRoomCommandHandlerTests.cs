using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.DeleteRoom;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class DeleteRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly DeleteRoomCommandHandler _handler;
    private readonly Room _existingRoom;
    private readonly Guid _roomId;

    public DeleteRoomCommandHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));

        var createResult = Room.Create(
            Name.From("TestRoom"),
            Capacity.From(50),
            RoomCode.From("VALID123"),
            DateTimeUtc.FromUtc(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc)));
        _existingRoom = createResult.Value!;
        _roomId = _existingRoom.Id;

        _roomRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingRoom);

        _handler = new DeleteRoomCommandHandler(
            _roomRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldSucceed()
    {
        var command = new DeleteRoomCommand(RoomId.From(_roomId), "Manual");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldReturnNotFoundError()
    {
        _roomRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var command = new DeleteRoomCommand(RoomId.From(Guid.NewGuid()), "Manual");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_InvalidDeletionReason_ShouldReturnError()
    {
        var command = new DeleteRoomCommand(RoomId.From(_roomId), "InvalidReason");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.InvalidDeletionReason", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_ActiveRoomWithInactivityReason_ShouldFail()
    {
        var command = new DeleteRoomCommand(RoomId.From(_roomId), "Inactivity");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.ActiveRoomCannotBeDeletedDueToInactivity", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AlreadyDeletedRoom_ShouldFail()
    {
        _existingRoom.Delete(DeletionReason.Manual, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new DeleteRoomCommand(RoomId.From(_roomId), "Manual");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.Deleted", result.Error!.Code);
    }
}
