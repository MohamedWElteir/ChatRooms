using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
