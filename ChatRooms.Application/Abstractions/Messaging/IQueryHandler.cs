using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
}
