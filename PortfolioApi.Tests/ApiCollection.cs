namespace PortfolioApi.Tests;

// Alle testklassen met [Collection("Api")] delen één PortfolioApiFactory.
// xUnit voert ze dan na elkaar uit, zodat ze elkaar in de testdatabase niet in de weg zitten.
[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<PortfolioApiFactory>
{
}