using System;
using Azure.Data.Tables;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorMapper
{
    public static Sensor Map(TableEntity entity)
    {
        return new Sensor
        {
            BoxId = entity.PartitionKey,
            Type = Enum.Parse<SensorType>(entity.RowKey),
            LastSeen = entity.GetDateTimeOffset(nameof(Sensor.LastSeen)) ?? DateTimeOffset.MinValue,
            Max = entity.GetDouble(nameof(Sensor.Max)) ?? 0,
            Min = entity.GetDouble(nameof(Sensor.Min)) ?? 0
        };
    }

    public static TableEntity Map(Sensor sensor)
    {
        var entity = new TableEntity(sensor.BoxId, sensor.Type.ToString("G"))
        {
            [nameof(Sensor.Max)] = sensor.Max, 
            [nameof(Sensor.Min)] = sensor.Min, 
            [nameof(Sensor.LastSeen)] = sensor.LastSeen
        };
        return entity;
    }
}