using ChatRooms.Application.Users.Queries;
using ChatRooms.Application.Users.Queries.GetUserById;
using ChatRooms.DTOs.Users;
using Moq;

namespace ChatRooms.Application.Tests.Users;

public sealed class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserQuery> _queryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _queryMock = new Mock<IUserQuery>();
        _handler = new GetUserByIdQueryHandler(_queryMock.Object);
    }

    [Fact]
    public async Task Handle_UserExists_ShouldReturnDto()
    {
        var id = Guid.NewGuid();
        var dto = new UserDto(id, "JohnDoe", "john@test.com", "Male", 1);
        _queryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(new GetUserByIdQuery(id), CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.Equal("JohnDoe", result.Name);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _queryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new GetUserByIdQuery(id), CancellationToken.None));
    }
}
