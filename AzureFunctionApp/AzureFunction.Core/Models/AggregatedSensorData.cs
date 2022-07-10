using System;

namespace AzureFunction.Core.Models;

public class AggregatedSensorData
{
    public string SensorBoxId { get; set; } = null!;

    public SensorType SensorType { get; set; }

    public AggregationType AggregationType { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public double Value { get; set; }
}

public enum AggregationType
{
    Min, Max, Mean, StandardDeviation
}