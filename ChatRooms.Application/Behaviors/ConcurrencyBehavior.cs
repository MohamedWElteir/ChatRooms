using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatRooms.Application.Behaviors;

public sealed class ConcurrencyBehavior<TRequest, TResponse>(
    ILogger<ConcurrencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex,
                "Concurrency conflict on {RequestType}", typeof(TRequest).Name);

            throw new ConcurrencyConflictException(
                "The resource was modified by another request. " +
                "Please refresh and try again.");
        }
    }
}