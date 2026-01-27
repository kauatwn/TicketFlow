using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;
using TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;
using TicketFlow.Domain.Repositories;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.Infrastructure.Persistence.Repositories;

namespace TicketFlow.Infrastructure.Extensions;

[ExcludeFromCodeCoverage(Justification = "Pure dependency injection configuration")]
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketFlowDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            });

            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketReadRepository, TicketReadRepository>();

        services.AddScoped<IShowRepository, ShowRepository>();
        services.AddScoped<IShowReadRepository, ShowReadRepository>();
    }
}