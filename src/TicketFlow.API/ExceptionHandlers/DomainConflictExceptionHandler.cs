using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.API.ExceptionHandlers;

public sealed partial class DomainConflictExceptionHandler(ILogger<DomainConflictException> logger) : IExceptionHandler
{
    public const string DefaultType = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
    public const string DefaultTitle = "Resource Conflict";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainConflictException conflictEx)
        {
            return false;
        }

        LogConflictError(conflictEx.Message);

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status409Conflict,
            Type = DefaultType,
            Title = DefaultTitle,
            Detail = conflictEx.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Resource conflict detected: {Message}")]
    private partial void LogConflictError(string message);
}