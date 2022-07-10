using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunction.App;

public class SensorFunction
{
    private readonly ISensorService _sensorService;
    private readonly ILogger<SensorFunction> _logger;

    public SensorFunction(ISensorService sensorService, ILogger<SensorFunction> logger)
    {
        _sensorService = sensorService;
        _logger = logger;
    }

    [FunctionName("GetSensor")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sensorBoxId = request.GetQueryParameterDictionary()["sensorBoxId"];
            var sensorType = Enum.Parse<SensorType>(request.GetQueryParameterDictionary()["sensorType"]);
            var sensor = await _sensorService.GetByBoxIdAndTypeAsync(sensorBoxId, sensorType);

            return sensor != null ? new OkObjectResult(sensor) : new NotFoundResult();
        }
        catch (Exception e) when (e is KeyNotFoundException or ArgumentException)
        {
            return new BadRequestResult();
        }
    }
}