using MediatR;

namespace ChatRooms.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{

}
