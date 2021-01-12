using Microsoft.Extensions.Configuration;

namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    public static class ConfigurationFactory
    {
        public static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder().AddJsonFile("appsettings.json", true, false).Build() as IConfiguration;
        }
    }
}