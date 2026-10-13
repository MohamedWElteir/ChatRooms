using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.LeaveRoom;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class LeaveRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly LeaveRoomCommandHandler _handler;
    private readonly Room _existingRoom;
    private readonly Guid _roomId;

    public LeaveRoomCommandHandlerTests()
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
        _existingRoom.Join(DateTimeUtc.FromUtc(DateTime.UtcNow));
        _roomId = _existingRoom.Id;

        _roomRepositoryMock.Setup(x => x.GetAsync(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingRoom);

        _handler = new LeaveRoomCommandHandler(
            _roomRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidLeave_ShouldSucceed()
    {
        var command = new LeaveRoomCommand(RoomId.From(_roomId));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _existingRoom.CurrentParticipantsCount);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldThrowKeyNotFoundException()
    {
        _roomRepositoryMock.Setup(x => x.GetAsync(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Room not found"));

        var command = new LeaveRoomCommand(RoomId.From(Guid.NewGuid()));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoParticipants_ShouldFail()
    {
        _existingRoom.Leave(DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new LeaveRoomCommand(RoomId.From(_roomId));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NoParticipantsToLeave", result.Error!.Code);
    }
}
