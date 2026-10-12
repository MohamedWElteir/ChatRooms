using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.RestoreRoom;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class RestoreRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly RestoreRoomCommandHandler _handler;
    private readonly Room _existingRoom;
    private readonly Guid _roomId;

    public RestoreRoomCommandHandlerTests()
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
        _existingRoom.Archive(DateTimeUtc.FromUtc(DateTime.UtcNow));
        _roomId = _existingRoom.Id;

        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingRoom);

        _handler = new RestoreRoomCommandHandler(
            _roomRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRestore_ShouldSucceed()
    {
        var command = new RestoreRoomCommand(RoomId.From(_roomId));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RoomStatus.Active, _existingRoom.Status);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldReturnNotFoundError()
    {
        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var command = new RestoreRoomCommand(RoomId.From(Guid.NewGuid()));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_NotArchived_ShouldFail()
    {
        var activeRoom = Room.Create(
            Name.From("ActiveRoom"),
            Capacity.From(50),
            RoomCode.From("VALID456"),
            DateTimeUtc.FromUtc(DateTime.UtcNow)).Value!;

        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeRoom);

        var command = new RestoreRoomCommand(RoomId.From(activeRoom.Id));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.OnlyArchivedCanBeRestored", result.Error!.Code);
    }
}
