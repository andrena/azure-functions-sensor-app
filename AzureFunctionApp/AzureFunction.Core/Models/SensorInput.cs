using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureFunction.Core.Models;

public class SensorInput
{
    public string SensorBoxId { get; set; } = null!;
        
    public DateTimeOffset Timestamp { get; set; }

    public IEnumerable<SensorData> Data { get; set; } = Enumerable.Empty<SensorData>();
}

public class SensorData
{
    public SensorType Type { get; set; }

    public IEnumerable<double> Values { get; set; } = Enumerable.Empty<double>();
}