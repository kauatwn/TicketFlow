using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.API.ExceptionHandlers;

public sealed partial class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public const string DefaultType = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
    public const string DefaultTitle = "Resource Not Found";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundEx)
        {
            return false;
        }

        LogResourceNotFound(notFoundEx.Message);

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound,
            Type = DefaultType,
            Title = DefaultTitle,
            Detail = notFoundEx.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Resource not found: {Message}")]
    private partial void LogResourceNotFound(string message);
}