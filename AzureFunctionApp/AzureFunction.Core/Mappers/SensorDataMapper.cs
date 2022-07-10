using System;
using Azure.Data.Tables;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorDataMapper
{
    public static TableEntity Map(AggregatedSensorData sensorData)
    {
        return new TableEntity(sensorData.SensorBoxId, Guid.NewGuid().ToString())
        {
            [nameof(AggregatedSensorData.SensorType)] = sensorData.SensorType.ToString("G"),
            [nameof(AggregatedSensorData.AggregationType)] = sensorData.AggregationType.ToString("G"),
            Timestamp = sensorData.CreatedAt,
            [nameof(AggregatedSensorData.Value)] = sensorData.Value
        };
    }
}