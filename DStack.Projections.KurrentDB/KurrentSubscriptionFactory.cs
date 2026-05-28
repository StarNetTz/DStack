using KurrentDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DStack.Projections.KurrentDB;

public class KurrentSubscriptionFactory : ISubscriptionFactory
{
    ILoggerFactory LoggerFactory;
    KurrentDBClient Client;

    public KurrentSubscriptionFactory(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        LoggerFactory = loggerFactory;
        var settings = KurrentDBClientSettings.Create(configuration["KurrentDB:ConnectionString"]);
        Client = new KurrentDBClient(settings);
    }
    public ISubscription Create()
    {
        return new KurrentSubscription(LoggerFactory.CreateLogger<KurrentSubscription>(), Client);
    }
}
