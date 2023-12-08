using System.Collections.Generic;
using System.Text;

namespace DStack.Projections.EventStoreDB;

public class EventStoreProjectionBuilder
{
    public static EventStoreProjection BuildProjectionDefinition(EventStoreProjectionParameters parameters)
    {
        var body = $"fromStreams({CreateSourceStreams(parameters.SourceStreamNames)}).when({{{CreateBody(parameters)}}})";
        return new EventStoreProjection { Name = parameters.Name, Source = body };
    }

        static string CreateSourceStreams(List<string> streamNames)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < streamNames.Count; i++)
            {
                if (IsNotFirstElement(i))
                    sb.Append(",");
                sb.Append($"'{streamNames[i]}'");
            }
            return sb.ToString();
        }

        static string CreateBody(EventStoreProjectionParameters parameters)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.EventsToInclude.Length; i++)
            {
                if (IsNotFirstElement(i))
                    sb.Append(",");
                sb.Append($"{parameters.EventsToInclude[i].Name}: function(s,e){{linkTo('{parameters.DestinationStreamName}', e);return s;}}");
            }
            return sb.ToString();
        }

            static bool IsNotFirstElement(int i)
            {
                return i > 0;
            }
}
