using ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;
using ChatRooms.Application.Rooms.Commands.CreateRoom;
using ChatRooms.Application.Rooms.Commands.DeleteRoom;
using ChatRooms.Application.Rooms.Commands.RenameRoom;
using ChatRooms.Application.Rooms.Queries.GetRoomByCode;
using ChatRooms.Application.Rooms.Queries.GetRoomById;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Presentation.Rooms.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatRooms.Presentation.Rooms;

[ApiController]
[Route("api/rooms")]
public sealed class RoomController(ISender sender) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(request.Name, request.Capacity);
        var room = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var room = await sender.Send(new GetRoomByIdQuery(id), cancellationToken);
        return Ok(room);
    }

    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(
        string code,
        CancellationToken cancellationToken)
    {
        var room = await sender.Send(new GetRoomByCodeQuery(code), cancellationToken);
        return Ok(room);
    }


    [HttpPatch("{id:guid}/name")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Rename(
        Guid id,
        [FromBody] RenameRoomRequest request,
        CancellationToken cancellationToken)
    {
        var newName = await sender.Send(
            new RenameRoomCommand(RoomId.From(id), request.NewName),
            cancellationToken);
        return Ok(newName);
    }

    [HttpPatch("{id:guid}/capacity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeCapacity(
        Guid id,
        [FromBody] ChangeCapacityRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ChangeRoomCapacityCommand(RoomId.From(id), request.NewCapacity),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
    Guid id,
    [FromBody] DeleteRoomRequest request,
    CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteRoomCommand(RoomId.From(id), request.Reason),
            cancellationToken);

        return NoContent();
    }
}