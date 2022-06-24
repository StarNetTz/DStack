using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DStack.Aggregates
{
    public class AggregateStateFactory
    {
        static readonly ConcurrentDictionary<Type, Type> LookupTable;

        static AggregateStateFactory()
        {
            LookupTable = new ConcurrentDictionary<Type, Type>();
        }

        public static IAggregateState CreateStateFor(Type aggregateType)
        {
            if (!LookupTable.TryGetValue(aggregateType, out Type aggStateType))
            {
                Assembly assemblyThatContainsAggregate = aggregateType.Assembly;
                string aggStateTypeName = string.Format("{0}State", aggregateType.FullName);
                aggStateType = assemblyThatContainsAggregate.GetType(aggStateTypeName);
                LookupTable.TryAdd(aggregateType, aggStateType);
            }
            return Activator.CreateInstance(aggStateType) as IAggregateState;
        }
    }
}