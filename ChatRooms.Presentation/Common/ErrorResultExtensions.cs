using ChatRooms.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatRooms.Presentation.Common;

public static class ErrorResultExtensions
{
    public static ObjectResult ToProblemDetails(this Error error)
    {
        var statusCode = error.Code.EndsWith("NotFound", StringComparison.Ordinal) ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;

        return new ObjectResult(new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Status = statusCode
        })
        { StatusCode = statusCode };
    }
}
