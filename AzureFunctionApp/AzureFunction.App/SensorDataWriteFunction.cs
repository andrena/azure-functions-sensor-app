using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunction.App;

public class SensorDataWriteFunction
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<SensorDataWriteFunction> _logger;

    public SensorDataWriteFunction(ISensorDataService sensorDataService, ILogger<SensorDataWriteFunction> logger)
    {
        _sensorDataService = sensorDataService;
        _logger = logger;
    }

    [FunctionName("SensorDataWriteFunction")]
    public async Task Run([QueueTrigger("validated-sensor-data")] AggregatedSensorData aggregatedSensorData)
    {
        await _sensorDataService.InsertAsync(aggregatedSensorData);
    }
}