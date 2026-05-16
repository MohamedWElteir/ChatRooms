using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class ChangeRoomCapacityCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ChangeRoomCapacityCommandHandler _handler;
    private readonly Room _existingRoom;
    private readonly Guid _roomId;

    public ChangeRoomCapacityCommandHandlerTests()
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

        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingRoom);

        _handler = new ChangeRoomCapacityCommandHandler(
            _roomRepositoryMock.Object,
            _dateTimeProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidChange_ShouldSucceed()
    {
        var command = new ChangeRoomCapacityCommand(RoomId.From(_roomId), 100);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldReturnNotFoundError()
    {
        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var command = new ChangeRoomCapacityCommand(RoomId.From(Guid.NewGuid()), 100);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_SameCapacity_ShouldNotRaiseEvent()
    {
        var command = new ChangeRoomCapacityCommand(RoomId.From(_roomId), 50);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ArchivedRoom_ShouldFail()
    {
        _existingRoom.Archive(DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new ChangeRoomCapacityCommand(RoomId.From(_roomId), 100);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NotActive", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_CapacityLessThanParticipants_ShouldFail()
    {
        _existingRoom.Join(DateTimeUtc.FromUtc(DateTime.UtcNow));
        _existingRoom.Join(DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new ChangeRoomCapacityCommand(RoomId.From(_roomId), 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NewCapacityCannotBeLessThanCurrentParticipants", result.Error!.Code);
    }
}
