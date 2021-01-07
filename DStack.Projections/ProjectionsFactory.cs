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
        readonly IServiceProvider Provider;

        public ProjectionsFactory(IServiceProvider provider)
        {
            Provider = provider;
        }

        public async Task<IList<IProjection>> Create(Assembly projectionsAssembly)
        {
            var type = typeof(IProjection);
            var ret = new List<IProjection>();
            var types = projectionsAssembly.GetTypes().Where(p => type.IsAssignableFrom(p)).ToList();
            foreach (var t in types)
                if (!IsInactive(t))
                    ret.Add(await Create(t));
            return ret;
        }

            static bool IsInactive(Type t)
            {
                var attrInfo = t.GetCustomAttribute(typeof(InactiveProjection)) as InactiveProjection;
                return attrInfo != null;
            }

        public async Task<IProjection> Create<T>()
        {
            var type = typeof(T);
            return await Create(type);
        }

        public async Task<IProjection> Create(Type type)
        {
            var proj = new Projection();
            SetNameAndStreamName(type, proj);
            await LoadCheckpoint(proj);
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
                proj.Checkpoint = await cr.Read($"{CheckpointsPrefix}-{proj.Name}");
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
                        return type.Name.Replace("Projection", "");
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
                subscription.EventAppearedCallback = proj.Project;
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