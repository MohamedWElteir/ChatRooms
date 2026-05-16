using ChatRooms.Application.Users.Commands.ChangeEmail;
using ChatRooms.Application.Users.Commands.ChangeGender;
using ChatRooms.Application.Users.Commands.CreateUser;
using ChatRooms.Application.Users.Commands.DeleteUser;
using ChatRooms.Application.Users.Commands.RenameUser;
using ChatRooms.Application.Users.Queries.GetUserByEmail;
using ChatRooms.Application.Users.Queries.GetUserById;
using ChatRooms.DTOs.Users;
using ChatRooms.Presentation.Common;
using ChatRooms.Presentation.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatRooms.Presentation.Users;

[ApiController]
[Route("api/users")]
public sealed class UserController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Name, request.Email, request.Gender, request.BirthDate);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return result.Error!.ToProblemDetails();

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpGet("by-email")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByEmailQuery(email), cancellationToken);
        return Ok(user);
    }

    [HttpPatch("{id:guid}/name")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Rename(
        Guid id,
        [FromBody] RenameUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RenameUserCommand(id, request.NewName),
            cancellationToken);

        if (result.IsFailure) return result.Error!.ToProblemDetails();

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeEmail(
        Guid id,
        [FromBody] ChangeEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeEmailCommand(id, request.NewEmail),
            cancellationToken);

        if (result.IsFailure) return result.Error!.ToProblemDetails();

        return NoContent();
    }

    [HttpPatch("{id:guid}/gender")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeGender(
        Guid id,
        [FromBody] ChangeGenderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeGenderCommand(id, request.NewGender),
            cancellationToken);

        if (result.IsFailure) return result.Error!.ToProblemDetails();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromBody] DeleteUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteUserCommand(id, request.Reason),
            cancellationToken);

        if (result.IsFailure) return result.Error!.ToProblemDetails();

        return NoContent();
    }
}
