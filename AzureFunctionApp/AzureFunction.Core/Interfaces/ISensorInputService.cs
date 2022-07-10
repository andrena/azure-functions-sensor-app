using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorInputService
{
    Task<IEnumerable<AggregatedSensorData>> ProcessInputAsync(SensorInput input);
}