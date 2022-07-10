using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Core.Services;

public class SensorInputService : ISensorInputService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ILogger<ISensorInputService> _logger;

    public SensorInputService(ISensorRepository sensorRepository, ILogger<ISensorInputService> logger)
    {
        _sensorRepository = sensorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AggregatedSensorData>> ProcessInputAsync(SensorInput input)
    {
        _logger.LogDebug($"Incoming raw sensor data: sensor id {input.SensorBoxId}");

        if (!input.Data.SelectMany(x => x.Values).Any())
        {
            return Enumerable.Empty<AggregatedSensorData>();
        }

        var aggregatedSensorData = new List<AggregatedSensorData>();
        foreach (var sensorData in input.Data)
        {
            var sensor = await _sensorRepository.GetByBoxIdAndTypeAsync(input.SensorBoxId, sensorData.Type);
            if (sensor == null)
            {
                throw new InvalidOperationException($"Sensor with box id {input.SensorBoxId} and type {sensorData.Type} not found.");
            }
            AddAggregatedData(input, aggregatedSensorData, sensor, sensorData);
        }

        return aggregatedSensorData;
    }

    private static void AddAggregatedData(SensorInput input, ICollection<AggregatedSensorData> aggregatedSensorData, Sensor sensor, SensorData sensorData)
    {
        aggregatedSensorData.Add(new AggregatedSensorData
        {
            SensorBoxId = sensor.BoxId,
            SensorType = sensor.Type,
            AggregationType = AggregationType.Mean,
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Average()
        });
        aggregatedSensorData.Add(new AggregatedSensorData
        {
            SensorBoxId = sensor.BoxId,
            SensorType = sensor.Type,
            AggregationType = AggregationType.Min,
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Min()
        });
        aggregatedSensorData.Add(new AggregatedSensorData
        {
            SensorBoxId = sensor.BoxId,
            SensorType = sensor.Type,
            AggregationType = AggregationType.Max,
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Max()
        });
        aggregatedSensorData.Add(new AggregatedSensorData
        {
            SensorBoxId = sensor.BoxId,
            SensorType = sensor.Type,
            AggregationType = AggregationType.StandardDeviation,
            CreatedAt = input.Timestamp,
            Value = sensorData.Values.Aggregate(0d, (a, x) => a + Math.Pow(x - sensorData.Values.Average(), 2)) / (sensorData.Values.Count() - 1)
        });
    }
}