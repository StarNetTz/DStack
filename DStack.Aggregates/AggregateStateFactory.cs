using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DStack.Aggregates
{
    public class AggregateStateFactory
    {
        static ConcurrentDictionary<Type, Type> LookupTable;

        static AggregateStateFactory()
        {
            LookupTable = new ConcurrentDictionary<Type, Type>();
        }

        public static IAggregateState CreateStateFor(Type aggregateType)
        {
            Type aggStateType = null;
            if (!LookupTable.TryGetValue(aggregateType, out aggStateType))
            {
                Assembly assemblyThatContainsAggregate = aggregateType.Assembly;
                string aggStateTypeName = string.Format("{0}State", aggregateType.FullName);
                aggStateType = assemblyThatContainsAggregate.GetType(aggStateTypeName);
                LookupTable.TryAdd(aggregateType, aggStateType);
            }
            var obj = Activator.CreateInstance(aggStateType);
            return obj as IAggregateState;
        }
    }
}