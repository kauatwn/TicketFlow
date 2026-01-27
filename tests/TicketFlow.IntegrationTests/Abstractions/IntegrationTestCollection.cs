namespace TicketFlow.IntegrationTests.Abstractions;

[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;