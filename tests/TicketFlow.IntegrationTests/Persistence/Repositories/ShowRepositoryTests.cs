using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Domain.Entities;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.Infrastructure.Persistence.Repositories;
using TicketFlow.IntegrationTests.Abstractions;

namespace TicketFlow.IntegrationTests.Persistence.Repositories;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class ShowRepositoryTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Add should persist new show to database")]
    public async Task Add_ShouldPersistShow()
    {
        // Arrange
        Show show = new("Rock Festival", DateTime.UtcNow.AddDays(30), 100, DateTime.UtcNow);

        // Act
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
            ShowRepository repository = new(context);

            repository.Add(show);
            await context.SaveChangesAsync();
        }

        // Assert
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
            Show? persistedShow = await context.Shows.FirstOrDefaultAsync(s => s.Id == show.Id);

            Assert.NotNull(persistedShow);
            Assert.Equal("Rock Festival", persistedShow.Title);
        }
    }
}