using System;
using System.Collections.Generic;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class EventStoreProjectionBuilderTests
{
    [Fact]
    public void DevicesProjection_is_created()
    {
        var p = new EventStoreProjectionParameters
        {
            Name = "ProjectionDevices",
            SourceStreamNames = new List<string> { "$ce-Locations" },
            DestinationStreamName = "cp-Devices",
            EventsToInclude = new Type[] { typeof(LocationOpened) }
        };
        var proj = EventStoreProjectionBuilder.BuildProjectionDefinition(p);

        string expected = "fromStreams('$ce-Locations').when({LocationOpened: function(s,e){linkTo('cp-Devices', e);return s;}})";
        Assert.Equal("ProjectionDevices", proj.Name);
        Assert.Equal(expected, proj.Source);
    }

    class LocationOpened
    {
    }
}