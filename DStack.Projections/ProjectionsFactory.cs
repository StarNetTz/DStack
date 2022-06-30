using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public class ProjectionsFactory : IProjectionsFactory
    {
        const string CheckpointsPrefix = "Checkpoints";
        const string ProjectionPrefix = "Projection";
        readonly IServiceProvider Provider;

        public ProjectionsFactory(IServiceProvider provider)
        {
            Provider = provider;
        }

        public async Task<IList<IProjection>> CreateAsync(Assembly projectionsAssembly)
        {
            var type = typeof(IProjection);
            var ret = new List<IProjection>();
            var types = projectionsAssembly.GetTypes().Where(p => type.IsAssignableFrom(p)).ToList();
            foreach (var t in types)
                if (IsActive(t))
                    ret.Add(await CreateAsync(t).ConfigureAwait(false));
            return ret;
        }

            static bool IsActive(Type t)
            {
                var attrInfo = t.GetCustomAttribute(typeof(InactiveProjection)) as InactiveProjection;
                return attrInfo == null;
            }

        public async Task<IProjection> CreateAsync<T>()
        {
            var type = typeof(T);
            return await CreateAsync(type).ConfigureAwait(false);
        }

        public async Task<IProjection> CreateAsync(Type type)
        {
            var proj = new Projection();
            SetNameAndStreamName(type, proj);
            await LoadCheckpoint(proj).ConfigureAwait(false);
            proj.CheckpointWriter = Provider.GetRequiredService<ICheckpointWriter>(); ;
            proj.Subscription = CreateSubscription(proj);
            proj.Handlers = CreateHandlers(type);
            return proj;
        }

            void SetNameAndStreamName(Type type, Projection proj)
            {
                var pi = GetProjectionInfo(type);
                proj.Name = pi.Name;
                proj.SubscriptionStreamName = pi.SubscriptionStreamName;
            }

            async Task LoadCheckpoint(Projection proj)
            {
                var cr = Provider.GetRequiredService<ICheckpointReader>();
                proj.Checkpoint = await cr.Read($"{CheckpointsPrefix}-{proj.Name}").ConfigureAwait(false);
            }

                ProjectionInfo GetProjectionInfo(Type type)
                {
                    return new ProjectionInfo
                    {
                        Name = GetProjectionName(type),
                        SubscriptionStreamName = GetSubscriptionStreamName(type)
                    };
                }

                    string GetProjectionName(Type type)
                    {
                        return type.Name.Replace(ProjectionPrefix, "");
                    }

                    string GetSubscriptionStreamName(Type type)
                    {
                        var attrInfo = type.GetCustomAttribute(typeof(SubscribesToStream)) as SubscribesToStream;
                        return attrInfo.Name;
                    }

            ISubscription CreateSubscription(Projection proj)
            {
                var subscription = Provider.GetRequiredService<ISubscriptionFactory>().Create();
                subscription.StreamName = proj.SubscriptionStreamName;
                subscription.EventAppearedCallback = proj.ProjectAsync;
                return subscription;
            }

            List<IHandler> CreateHandlers(Type type)
            {
                Type[] projectionTypesThatPointToHandlers = (
                    from iType in type.GetInterfaces()
                    where iType.IsGenericType
                    && iType.GetGenericTypeDefinition() == typeof(IHandledBy<>)
                    select iType.GetGenericArguments()[0]).ToArray();
                var handlers = new List<IHandler>();
                foreach (var t in projectionTypesThatPointToHandlers)
                    handlers.Add(Provider.GetRequiredService<IHandlerFactory>().Create(t));
                return handlers;
            }

        class ProjectionInfo
        {
            public string Name { get; set; }
            public string SubscriptionStreamName { get; set; }
        }
    }
}