using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.ExceptionHandlers;

public sealed partial class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public const string DefaultType = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    public const string DefaultTitle = "Internal Server Error";
    public const string DefaultDetail = "An unexpected internal error occurred.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogCriticalError(exception, exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = DefaultType,
            Title = DefaultTitle,
            Detail = DefaultDetail
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception has occurred: {Message}")]
    private partial void LogCriticalError(Exception ex, string message);
}