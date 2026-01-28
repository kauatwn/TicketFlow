using System.Diagnostics.CodeAnalysis;
using TicketFlow.API.Extensions;
using TicketFlow.Application.Extensions;
using TicketFlow.Infrastructure.Extensions;

[assembly: ExcludeFromCodeCoverage(Justification = "API layer is a thin adapter without business logic. Integration tests are not present in this solution.")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiExceptionHandlers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Ticket Flow API"));

    await app.ApplyMigrationsAsync();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();