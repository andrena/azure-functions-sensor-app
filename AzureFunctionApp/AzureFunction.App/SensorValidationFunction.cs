using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunction.App;

public class SensorValidationFunction
{
    private readonly ISensorValidationService _validationService;
    private readonly ILogger<SensorValidationFunction> _logger;

    public SensorValidationFunction(ISensorValidationService validationService, ILogger<SensorValidationFunction> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    [FunctionName("SensorValidation")]
    public async Task Run(
        [QueueTrigger("aggregated-sensor-data")] AggregatedSensorData aggregatedSensorData,
        [Queue("validated-sensor-data")] IAsyncCollector<AggregatedSensorData> output,
        CancellationToken cancellationToken)
    {
        await _validationService.ValidateSensorDataAsync(aggregatedSensorData);

        await output.AddAsync(aggregatedSensorData, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }

    [FunctionName("InactiveSensors")]
    public async Task GetInactiveSensors([TimerTrigger("0 */1 * * * *")] TimerInfo timer)
    {
        await _validationService.CheckSensorsAndAlarmsAsync();
    }
}