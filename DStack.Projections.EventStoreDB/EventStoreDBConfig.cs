using Microsoft.Extensions.Configuration;
using EventStore.ClientAPI.SystemData;
using System.Net;
using System.IO;
using System;

namespace DStack.Projections.EventStoreDB
{
    public class EventStoreDBConfig
    {
        public static UserCredentials UserCredentials { get; set; }
        public static IPEndPoint HttpEndpoint { get; set; }
        public static string ConnectionString { get; set; }
        public static string HttpSchema { get; set; }

        static EventStoreDBConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var conf = builder.Build();

            UserCredentials = new UserCredentials(conf["EventStoreDB:ProjectionsManager:Username"], conf["EventStoreDB:ProjectionsManager:Password"]);
            var url = conf["EventStoreDB:ProjectionsManager:Url"];

         
            Uri myUri = new Uri(url);
            var ip = Dns.GetHostAddresses(myUri.Host)[0];
            int httpPort = myUri.Port;

            HttpEndpoint = new IPEndPoint(ip, httpPort);
            ConnectionString = conf["EventStoreDB:ConnectionString"];
            HttpSchema = myUri.Scheme;
        }
    }
}
