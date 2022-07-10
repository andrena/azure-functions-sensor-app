using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorValidationService
{
    Task ValidateSensorDataAsync(AggregatedSensorData input);
    Task CheckSensorsAndAlarmsAsync();
}