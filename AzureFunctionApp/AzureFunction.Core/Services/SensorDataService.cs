using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Services;

public class SensorDataService: ISensorDataService
{
    private readonly ISensorDataRepository _sensorDataRepository;

    public SensorDataService(ISensorDataRepository sensorDataRepository)
    {
        _sensorDataRepository = sensorDataRepository;
    }

    public Task InsertAsync(AggregatedSensorData aggregatedSensorData)
    {
        return _sensorDataRepository.InsertAsync(aggregatedSensorData);
    }
}