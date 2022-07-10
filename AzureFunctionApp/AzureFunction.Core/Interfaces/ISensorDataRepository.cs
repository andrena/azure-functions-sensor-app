using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;

public interface ISensorDataRepository
{
    public Task InsertAsync(AggregatedSensorData aggregatedSensorData);
}