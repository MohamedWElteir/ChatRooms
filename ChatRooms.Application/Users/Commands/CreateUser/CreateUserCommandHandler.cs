using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var createResult = User.Create(
            name: command.Name,
            email: command.Email,
            gender: command.Gender,
            birthDate: command.BirthDate,
            occurredAt: dateTimeProvider.UtcNow);

        if (createResult.IsFailure) return createResult.Error!;

        var user = createResult.Value!;
        await userRepository.Add(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UserDto(
            Id: user.Id,
            Name: user.Name,
            Email: user.Email,
            Gender: user.Gender.ToString(),
            Version: user.Version);
    }
}
