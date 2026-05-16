using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface IQuery<TResult> : IRequest<TResult>
{
}
