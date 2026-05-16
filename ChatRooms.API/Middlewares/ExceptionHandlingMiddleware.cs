using ChatRooms.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ChatRooms.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ConcurrencyConflictException ex)
        {
            await HandleConflictAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleNotFoundAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception.");
            await HandleServerErrorAsync(context);
        }
    }

    private static Task HandleConflictAsync(HttpContext context, InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Concurrency Conflict",
            Detail = ex.Message,
            Status = StatusCodes.Status409Conflict
        });
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status422UnprocessableEntity,
            Extensions = { ["errors"] = ex.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) }
        });
    }

    private static Task HandleNotFoundAsync(HttpContext context, KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Resource Not Found",
            Detail = ex.Message,
            Status = StatusCodes.Status404NotFound
        });
    }

    private static Task HandleServerErrorAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
        });
    }
}