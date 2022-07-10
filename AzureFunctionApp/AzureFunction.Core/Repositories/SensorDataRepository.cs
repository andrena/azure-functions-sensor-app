using System.Threading.Tasks;
using Azure.Data.Tables;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Repositories;

public class SensorDataRepository : ISensorDataRepository
{
    private readonly string _tableName;
    private readonly TableServiceClient _client;
        
    public SensorDataRepository(string connectionString, string tableName)
    {
        _tableName = tableName;
        _client = new TableServiceClient(connectionString);
    }

    public async Task InsertAsync(AggregatedSensorData aggregatedSensorData)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        await table.UpsertEntityAsync(SensorDataMapper.Map(aggregatedSensorData));
    }
}