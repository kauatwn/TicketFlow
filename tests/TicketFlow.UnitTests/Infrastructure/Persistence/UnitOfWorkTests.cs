using Microsoft.EntityFrameworkCore;
using Moq;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Infrastructure.Persistence;

namespace TicketFlow.UnitTests.Infrastructure.Persistence;

[Trait("Category", "Unit")]
public class UnitOfWorkTests
{
    [Fact(DisplayName = "Should throw ConcurrencyException when DbUpdateConcurrencyException occurs")]
    public async Task CommitAsync_ShouldThrowConcurrencyException_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        // Arrange
        DbContextOptions<TicketFlowDbContext> options = new DbContextOptionsBuilder<TicketFlowDbContext>()
            .UseInMemoryDatabase(databaseName: "UnitOfWork_Test_Db")
            .Options;

        Mock<TicketFlowDbContext> contextMock = new(options);
        
        contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict", []));

        UnitOfWork unitOfWork = new(contextMock.Object);

        // Act
        Task Act() => unitOfWork.CommitAsync(CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<ConcurrencyException>(Act);
        Assert.Equal("The data was modified by another user while you were trying to save. Please refresh and try again.", exception.Message);
    }

    [Fact(DisplayName = "Should complete successfully when no concurrency exception occurs")]
    public async Task CommitAsync_ShouldCompleteSuccessfully_WhenNoConcurrencyException()
    {
        // Arrange
        DbContextOptions<TicketFlowDbContext> options = new DbContextOptionsBuilder<TicketFlowDbContext>()
            .UseInMemoryDatabase(databaseName: "UnitOfWork_Success_Db")
            .Options;

        Mock<TicketFlowDbContext> contextMock = new(options);

        contextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        UnitOfWork unitOfWork = new(contextMock.Object);

        // Act
        await unitOfWork.CommitAsync(CancellationToken.None);

        // Assert
        contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}