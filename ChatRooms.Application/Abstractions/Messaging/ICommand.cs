using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface ICommand<TResult> : IRequest<TResult>
{
}
