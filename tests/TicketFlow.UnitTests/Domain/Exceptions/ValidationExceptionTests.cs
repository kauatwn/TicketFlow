using TicketFlow.Domain.Exceptions;

namespace TicketFlow.UnitTests.Domain.Exceptions;

[Trait("Category", "Unit")]
public class ValidationExceptionTests
{
    [Fact(DisplayName = "Should initialize with single field error")]
    public void Constructor_ShouldInitialize_WithSingleFieldError()
    {
        // Arrange
        const string field = "TestField";
        const string error = "TestError";

        // Act
        ValidationException exception = new(field, error);

        // Assert
        Assert.Equal(ValidationException.DefaultErrorMessage, exception.Message);
        Assert.Single(exception.Errors);
        Assert.True(exception.Errors.ContainsKey(field));
        Assert.Contains(error, exception.Errors[field]);
    }

    [Fact(DisplayName = "Should initialize with dictionary of errors")]
    public void Constructor_ShouldInitialize_WithDictionary()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", ["Error1", "Error2"] },
            { "Field2", ["Error3"] }
        };

        // Act
        ValidationException exception = new(errors);

        // Assert
        Assert.Equal(ValidationException.DefaultErrorMessage, exception.Message);
        Assert.Equal(errors, exception.Errors);
    }
}