using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.API.ExceptionHandlers;

public sealed partial class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public const string DefaultType = "https://tools.ietf.org/html/rfc4918#section-11.2";
    public const string DefaultTitle = "Domain Rule Violation";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainEx)
        {
            return false;
        }

        LogDomainValidationError(domainEx, domainEx.Message);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Type = DefaultType,
            Title = DefaultTitle,
            Detail = domainEx.Message
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Domain validation error: {Message}")]
    private partial void LogDomainValidationError(Exception ex, string message);
}