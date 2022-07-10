using System;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunction.App;

public class SensorInputFunction
{
    private readonly ISensorInputService _inputService;
    private readonly ILogger<SensorInputFunction> _logger;

    public SensorInputFunction(ISensorInputService inputService, ILogger<SensorInputFunction> logger)
    {
        _inputService = inputService;
        _logger = logger;
    }

    [FunctionName("SensorInput")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] SensorInput input,
        [Queue("aggregated-sensor-data")] IAsyncCollector<AggregatedSensorData> output,
        CancellationToken cancellationToken)
    {
        try
        {
            var aggregatedSensorData = await _inputService.ProcessInputAsync(input);
            foreach (var sensorData in aggregatedSensorData)
            {
                await output.AddAsync(sensorData, cancellationToken);
            }
            await output.FlushAsync(cancellationToken);

            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing function 'Sensorinput'. Stack trace: {StackTrace}", e.StackTrace);
            throw;
        }
    }
}