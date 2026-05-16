using ChatRooms.Application.Users.Queries;
using ChatRooms.Application.Users.Queries.GetUserByEmail;
using ChatRooms.DTOs.Users;
using Moq;

namespace ChatRooms.Application.Tests.Users;

public sealed class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserQuery> _queryMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _queryMock = new Mock<IUserQuery>();
        _handler = new GetUserByEmailQueryHandler(_queryMock.Object);
    }

    [Fact]
    public async Task Handle_UserExists_ShouldReturnDto()
    {
        var dto = new UserDto(Guid.NewGuid(), "JohnDoe", "john@test.com", "Male", 1);
        _queryMock.Setup(x => x.GetByEmailAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(new GetUserByEmailQuery("john@test.com"), CancellationToken.None);

        Assert.Equal("john@test.com", result.Email);
        Assert.Equal("JohnDoe", result.Name);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        _queryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new GetUserByEmailQuery("unknown@test.com"), CancellationToken.None));
    }
}
