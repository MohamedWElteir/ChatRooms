using ChatRooms.Application.Abstractions.Common;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Application.Rooms.Commands.CreateRoom;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using Moq;

namespace ChatRooms.Application.Tests.Rooms;

public sealed class CreateRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenerator<RoomCode>> _codeGeneratorMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly CreateRoomCommandHandler _handler;

    public CreateRoomCommandHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _codeGeneratorMock = new Mock<IGenerator<RoomCode>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc));
        _codeGeneratorMock.Setup(x => x.Generate()).Returns(RoomCode.From("VALID123"));

        _handler = new CreateRoomCommandHandler(
            _roomRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _codeGeneratorMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateRoomAndReturnDto()
    {
        var command = new CreateRoomCommand("GeneralChat", 50);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("GeneralChat", result.Value.Name);
        Assert.Equal(50, result.Value.Capacity);
        Assert.Equal("VALID123", result.Value.Code);
        Assert.Equal("Active", result.Value.Status);

        _roomRepositoryMock.Verify(x => x.Add(It.IsAny<Room>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldThrowArgumentException()
    {
        var command = new CreateRoomCommand("", 50);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidCapacity_ShouldThrowArgumentException()
    {
        var command = new CreateRoomCommand("GeneralChat", 0);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
