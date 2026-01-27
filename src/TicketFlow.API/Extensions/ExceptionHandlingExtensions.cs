using System.Diagnostics.CodeAnalysis;
using TicketFlow.API.ExceptionHandlers;

namespace TicketFlow.API.Extensions;

[ExcludeFromCodeCoverage(Justification = "Pure dependency injection configuration")]
public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddApiExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<ValidationExceptionHandler>();

        services.AddExceptionHandler<DomainConflictExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();

        services.AddExceptionHandler<DomainExceptionHandler>();

        services.AddExceptionHandler<UnhandledExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}