using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Users.Commands.RenameUser;

public sealed class RenameUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RenameUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RenameUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(command.Id, cancellationToken);
        if (user is null) return UserErrors.NotFound;

        var renameResult = user.Rename(command.NewName, dateTimeProvider.UtcNow);
        if (renameResult.IsFailure) return renameResult.Error!;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Name.Value;
    }
}
