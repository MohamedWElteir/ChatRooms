using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ChatRooms.Application.Tests.Behaviors;

public sealed record ValidationTestCommand : ICommand<string>;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ShouldInvokeNextAndReturnResult()
    {
        var behavior = new ValidationBehavior<ValidationTestCommand, string>([]);
        var command = new ValidationTestCommand();
        static Task<string> next(CancellationToken _ = default) => Task.FromResult("success");

        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldInvokeNextAndReturnResult()
    {
        var validatorMock = new Mock<IValidator<ValidationTestCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<ValidationTestCommand, string>([validatorMock.Object]);
        var command = new ValidationTestCommand();
        static Task<string> next(CancellationToken _ = default) => Task.FromResult("success");

        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.Equal("success", result);
        validatorMock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<ValidationTestCommand>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new("Property", "Error message")
        };

        var validatorMock = new Mock<IValidator<ValidationTestCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidationTestCommand, string>([validatorMock.Object]);
        var command = new ValidationTestCommand();
       static Task<string> next(CancellationToken _ = default) => Task.FromResult("success");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(command, next, CancellationToken.None));

        Assert.NotEmpty(ex.Errors);
    }
}
