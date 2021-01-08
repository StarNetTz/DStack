using DStack.Projections.EventStoreDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.ES.IntegrationTests
{

    public class ESProjectionsFactoryTests
    {
     
        IProjectionsFactory ProjectionsFactory;

        public ESProjectionsFactoryTests()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, false).Build() as IConfiguration;
            services.AddSingleton(configuration);
            services.AddTransient<IHandlerFactory, StubHandlerFactory>();
            services.AddTransient<ILoggerFactory, NullLoggerFactory>();

            services.AddTransient<ICheckpointReader, StubCheckpointReader>();
            services.AddTransient<ICheckpointWriter, StubCheckpointWriter>();

            services.AddTransient<ISubscriptionFactory, ESSubscriptionGRPCFactory>();
            services.AddTransient<IProjectionsFactory, ProjectionsFactory>();
            var prov = services.BuildServiceProvider();

            ProjectionsFactory = prov.GetRequiredService<IProjectionsFactory>();
        }

        [Fact]
        public async Task can_create_test_projection_and_project()
        {

            var proj = await ProjectionsFactory.Create<TestProjection>();
           // await new ESDataGenerator().WriteEventsToStore(1);
           
            await proj.Start();

            await Task.Delay(200000);
            Assert.True(proj.Checkpoint.Value > 0);
        }

            async Task PreloadProjectionsSubscription()
            {
                
            }
    }
}