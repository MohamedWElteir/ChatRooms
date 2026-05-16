using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Application.Users.Commands.ChangeGender;

public sealed class ChangeGenderCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ChangeGenderCommand, Result>
{
    public async Task<Result> Handle(ChangeGenderCommand command, CancellationToken cancellationToken)
    {
        var userId = UserId.From(command.Id);
        var user = await userRepository.GetById(userId, cancellationToken);
        if (user is null) return UserErrors.NotFound;

        var result = user.ChangeGender(command.NewGender, DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
