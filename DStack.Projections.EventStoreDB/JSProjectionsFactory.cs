using EventStore.Client;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DStack.Projections.EventStoreDB.Utils
{
    public interface IJSProjectionsFactory
    {
        public Task CreateProjections();
        public void AddProjection(string name, string srcCode);
    }

    public class JSProjectionsFactory : IJSProjectionsFactory
    {
       EventStoreProjectionManagementClient Cli;

        public Dictionary<string, string> Projections { get; set; }

        public JSProjectionsFactory(IConfiguration conf)
        {
            var settings = EventStoreClientSettings.Create(conf["EventStoreDB:ConnectionString"]);
            Cli = new EventStore.Client.EventStoreProjectionManagementClient(settings);
            Projections = new Dictionary<string, string>();
        }

        public async Task CreateProjections()
        {
            var projections = await Cli.ListAllAsync().ToListAsync();
            var projectionNames = (from p in projections select p.Name).ToList();
            var newProjections = GetNewProjectionNames(projectionNames);
            
            foreach (var kv in newProjections)
            {
                await Cli.CreateContinuousAsync(kv.Key, kv.Value).ConfigureAwait(false);
                await Cli.UpdateAsync(kv.Key, kv.Value, emitEnabled : true).ConfigureAwait(false);
            }
               
        }   

            Dictionary<string, string> GetNewProjectionNames(List<string> existing)
            {
                return (from kv in Projections where !existing.Contains(kv.Key) select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            }

        public void AddProjection(string name, string srcCode)
        {
            Projections.Add(name, srcCode);
        }
    }
}