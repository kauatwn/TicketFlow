using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.API.ExceptionHandlers;

public sealed partial class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public const string DefaultType = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public const string DefaultDetail = "See the errors property for details.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationEx)
        {
            return false;
        }

        LogValidationFailure(validationEx.Errors.Count);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        ValidationProblemDetails problemDetails = new(validationEx.Errors)
        {
            Type = DefaultType,
            Detail = DefaultDetail
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request validation failed with {Count} errors.")]
    private partial void LogValidationFailure(int count);
}