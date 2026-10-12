using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.ArchiveRoom;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class ArchiveRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ArchiveRoomCommandHandler _handler;
    private readonly Room _existingRoom;
    private readonly Guid _roomId;

    public ArchiveRoomCommandHandlerTests()
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

        _handler = new ArchiveRoomCommandHandler(
            _roomRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidArchive_ShouldSucceed()
    {
        var command = new ArchiveRoomCommand(RoomId.From(_roomId));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RoomStatus.Archived, _existingRoom.Status);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoomNotFound_ShouldReturnNotFoundError()
    {
        _roomRepositoryMock.Setup(x => x.GetById(It.IsAny<RoomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var command = new ArchiveRoomCommand(RoomId.From(Guid.NewGuid()));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Room.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_AlreadyArchived_ShouldSucceedIdempotently()
    {
        _existingRoom.Archive(DateTimeUtc.FromUtc(DateTime.UtcNow));

        var command = new ArchiveRoomCommand(RoomId.From(_roomId));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
