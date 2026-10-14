using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Exceptions;
using ChatRooms.Infrastructure.Behaviors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatRooms.Infrastructure.Tests.Behaviors;

public sealed record ConcurrencyTestCommand : ICommand<string>;

public sealed class ConcurrencyBehaviorTests
{
    private readonly ConcurrencyBehavior<ConcurrencyTestCommand, string> _behavior;

    public ConcurrencyBehaviorTests()
    {
        _behavior = new ConcurrencyBehavior<ConcurrencyTestCommand, string>(
            NullLogger<ConcurrencyBehavior<ConcurrencyTestCommand, string>>.Instance);
    }

    [Fact]
    public async Task Handle_NoException_ShouldInvokeNextAndReturnResult()
    {
        var command = new ConcurrencyTestCommand();
        static Task<string> next(CancellationToken _ = default) => Task.FromResult("success");

        var result = await _behavior.Handle(command, next, CancellationToken.None);

        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_DbUpdateConcurrencyException_ShouldThrowConcurrencyConflictException()
    {
        var command = new ConcurrencyTestCommand();
        static Task<string> next(CancellationToken _ = default) => throw new DbUpdateConcurrencyException();

        var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            _behavior.Handle(command, next, CancellationToken.None));

        Assert.Contains("modified by another request", ex.Message);
    }
}
