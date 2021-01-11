using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    public class JSProjectionsFactoryTests
    {
        [Fact]
        public async Task CanCreateProjectionUsingSimpleInjector()
        {
           
            var provider = CreateServiceProvider();
            var fact = provider.GetRequiredService<IJSProjectionsFactory>();
            fact.AddProjection("IntegrationsTestProjection", "fromStreams('$ce-Competitions').when({CompetitionAdded: function(s,e){linkTo('cp-AssociationsOverview', e);return s;},CompetitionRenamed: function(s,e){linkTo('cp-AssociationsOverview', e);return s;},CompetitionsAssociationChanged: function(s,e){linkTo('cp-AssociationsOverview', e);return s;},CompetitionsRankChanged: function(s,e){linkTo('cp-AssociationsOverview', e);return s;}})");
            await fact.CreateProjections();
        }

            IServiceProvider CreateServiceProvider()
            {
                var services = new ServiceCollection();
                services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
                services.AddTransient(typeof(ILogger<>), typeof(Logger<>));
                services.AddTransient<IJSProjectionsFactory, JSProjectionsFactory>();
                return services.BuildServiceProvider();
            }
    }
}