using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.Infrastructure.Persistence.Seed;

namespace TicketFlow.API.Extensions;

[ExcludeFromCodeCoverage]
public static partial class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger("TicketFlow.Database");

        var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
        try
        {
            LogMigrationStart(logger);
            await context.Database.MigrateAsync();
            LogMigrationSuccess(logger);

            LogSeedingStart(logger);
            await TicketFlowSeeder.SeedAsync(context);
            LogSeedingSuccess(logger);
        }
        catch (Exception ex)
        {
            LogDatabaseCriticalError(logger, ex);
            throw;
        }
    }

    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Information,
        Message = "Starting database migration...")]
    private static partial void LogMigrationStart(ILogger logger);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Information,
        Message = "Database migrations applied successfully.")]
    private static partial void LogMigrationSuccess(ILogger logger);

    [LoggerMessage(
        EventId = 102,
        Level = LogLevel.Information,
        Message = "Seeding database with initial data...")]
    private static partial void LogSeedingStart(ILogger logger);

    [LoggerMessage(
        EventId = 103,
        Level = LogLevel.Information,
        Message = "Database seeding completed successfully.")]
    private static partial void LogSeedingSuccess(ILogger logger);

    [LoggerMessage(
        EventId = 104,
        Level = LogLevel.Critical,
        Message = "Critical failure initializing database (Migration/Seed).")]
    private static partial void LogDatabaseCriticalError(ILogger logger, Exception ex);
}