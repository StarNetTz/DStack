using System;
using System.Collections.Generic;
using Xunit;

namespace DStack.Projections.KurrentDB.IntegrationTests;

public class KurrentProjectionBuilderTests
{
    [Fact]
    public void DevicesProjection_is_created()
    {
        var p = new KurrentProjectionParameters
        {
            Name = "ProjectionDevices",
            SourceStreamNames = new List<string> { "$ce-Locations" },
            DestinationStreamName = "cp-Devices",
            EventsToInclude = new Type[] { typeof(LocationOpened) }
        };
        var proj = KurrentProjectionBuilder.BuildProjectionDefinition(p);

        string expected = "fromStreams('$ce-Locations').when({LocationOpened: function(s,e){linkTo('cp-Devices', e);return s;}})";
        Assert.Equal("ProjectionDevices", proj.Name);
        Assert.Equal(expected, proj.Source);
    }

    class LocationOpened
    {
    }
}
