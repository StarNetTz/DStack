using KurrentDB.Client;

namespace DStack.Projections.KurrentDB.IntegrationTests;

public static class KurrentDBClientFactory
{
    public static KurrentDBClient CreateKurrentDBClient()
    {
        var configuration = ConfigurationFactory.CreateConfiguration();
        var settings = KurrentDBClientSettings.Create(configuration["KurrentDB:ConnectionString"]);
        var cli = new KurrentDBClient(settings);
        return cli;
    }
}
