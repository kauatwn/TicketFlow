using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using TicketFlow.Application.Behaviors;
using ValidationException = TicketFlow.Domain.Exceptions.ValidationException;

namespace TicketFlow.UnitTests.Application.Behaviors;

[Trait("Category", "Unit")]
public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestCommand>> _validatorMock = new();
    private readonly ValidationBehavior<TestCommand, Unit> _sut;

    public ValidationBehaviorTests()
    {
        _sut = new ValidationBehavior<TestCommand, Unit>([_validatorMock.Object]);
    }

    public record TestCommand : IRequest<Unit>;

    [Fact(DisplayName = "Should throw ValidationException when validation fails")]
    public async Task Handle_ShouldThrowValidationException_WhenErrorsExist()
    {
        // Arrange
        List<ValidationFailure> failures = [new("Property1", "Error message 1")];

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        Task<Unit> Next(CancellationToken _ = default) => Task.FromResult(Unit.Value);

        // Act
        Task Act() => _sut.Handle(new TestCommand(), Next, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(Act);
        Assert.True(exception.Errors.ContainsKey("Property1"));
    }

    [Fact(DisplayName = "Should call next delegate when validation succeeds")]
    public async Task Handle_ShouldCallNext_WhenNoErrors()
    {
        // Arrange
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        bool nextCalled = false;

        Task<Unit> Next(CancellationToken _ = default)
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        await _sut.Handle(new TestCommand(), Next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact(DisplayName = "Should group multiple errors by property name correctly")]
    public async Task Handle_ShouldGroupMultipleErrorsByProperty_WhenMultipleValidationFailures()
    {
        // Arrange
        Mock<IValidator<TestCommand>> validator1Mock = new();
        Mock<IValidator<TestCommand>> validator2Mock = new();

        ValidationBehavior<TestCommand, Unit> behavior = new([validator1Mock.Object, validator2Mock.Object]);

        List<ValidationFailure> failures1 =
        [
            new("Property1", "Error message 1"),
            new("Property1", "Error message 2"),
            new("Property2", "Error message 3")
        ];

        List<ValidationFailure> failures2 =
        [
            new("Property1", "Error message 4"),
            new("Property3", "Error message 5")
        ];

        validator1Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures1));

        validator2Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures2));

        Task<Unit> Next(CancellationToken _ = default) => Task.FromResult(Unit.Value);

        // Act
        Task Act() => behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(Act);

        Assert.True(exception.Errors.ContainsKey("Property1"));
        Assert.Equal(3, exception.Errors["Property1"].Length);

        Assert.True(exception.Errors.ContainsKey("Property2"));
        Assert.Single(exception.Errors["Property2"]);

        Assert.True(exception.Errors.ContainsKey("Property3"));
        Assert.Single(exception.Errors["Property3"]);
    }

    [Fact(DisplayName = "Should call next delegate when no validators are provided")]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsProvided()
    {
        // Arrange
        ValidationBehavior<TestCommand, Unit> behaviorWithNoValidators = new([]);
        bool nextCalled = false;

        Task<Unit> Next(CancellationToken _ = default)
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        }

        // Act
        await behaviorWithNoValidators.Handle(new TestCommand(), Next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
    }
}