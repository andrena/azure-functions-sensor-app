using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorService
{
    Task<Sensor?> GetByBoxIdAndTypeAsync(string sensorBoxId, SensorType sensorType);
}