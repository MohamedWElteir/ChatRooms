using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface ICommand<out TResult> : IRequest<TResult> 
{
}
