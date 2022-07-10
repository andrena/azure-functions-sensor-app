using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Services;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;

    public SensorService(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    public Task<Sensor?> GetByBoxIdAndTypeAsync(string sensorBoxId, SensorType sensorType)
    {
        return _sensorRepository.GetByBoxIdAndTypeAsync(sensorBoxId, sensorType);
    }
}