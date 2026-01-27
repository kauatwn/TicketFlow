using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = TicketFlow.Domain.Exceptions.ValidationException;

namespace TicketFlow.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = [.. validationResults.SelectMany(r => r.Errors)];

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        Dictionary<string, string[]> errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        throw new ValidationException(errors);
    }
}