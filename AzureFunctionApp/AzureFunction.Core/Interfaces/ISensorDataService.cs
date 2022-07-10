using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorDataService
{
    Task InsertAsync(AggregatedSensorData aggregatedSensorData);
}