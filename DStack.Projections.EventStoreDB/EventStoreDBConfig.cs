using Microsoft.Extensions.Configuration;
using EventStore.ClientAPI.SystemData;
using System.Net;
using System.IO;

namespace DStack.Projections.EventStoreDB
{
    public class EventStoreDBConfig
    {
        public static UserCredentials UserCredentials { get; set; }
        public static IPEndPoint HttpEndpoint { get; set; }

        public static string ConnectionString { get; set; }
        public static string LegacyConnectionString { get; set; }

        static EventStoreDBConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var conf = builder.Build();

            UserCredentials = new UserCredentials(conf["EventStoreDB:ProjectionsManager:Username"], conf["EventStoreDB:ProjectionsManager:Password"]);
            var serverAddres = IPAddress.Parse(conf["EventStoreDB:ProjectionsManager:Url"]);
            int httpPort = int.Parse(conf["EventStoreDB:ProjectionsManager:Port"]);
            HttpEndpoint = new IPEndPoint(serverAddres, httpPort);
            ConnectionString = conf["EventStoreDB:ConnectionString"];
            LegacyConnectionString = conf["EventStoreDB:LegacyConnectionString"];
        }
    }
}
