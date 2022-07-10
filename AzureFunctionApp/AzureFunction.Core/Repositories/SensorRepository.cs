using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly string _tableName;
    private readonly TableServiceClient _client;

    public SensorRepository(string connectionString, string tableName)
    {
        _tableName = tableName;
        _client = new TableServiceClient(connectionString);
    }

    public async Task<Sensor?> GetByBoxIdAndTypeAsync(string boxId, SensorType type)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        var sensorTypeString = type.ToString("G");
        var entityPages = table.QueryAsync<TableEntity>(x => x.PartitionKey == boxId && x.RowKey == sensorTypeString);
        return (await entityPages.AsPages().FirstOrDefaultAsync())?.Values.Select(SensorMapper.Map).FirstOrDefault();
    }

    public async Task<IEnumerable<Sensor>> GetAllAsync()
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        var entityPages = table.QueryAsync<TableEntity>();

        return await entityPages
            .AsPages()
            .SelectMany(p => p.Values.Select(SensorMapper.Map).ToAsyncEnumerable())
            .ToListAsync();
    }

    public async Task InsertAsync(Sensor sensor)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        var entity = SensorMapper.Map(sensor);
        await table.UpsertEntityAsync(entity);
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        var entity = SensorMapper.Map(sensor);
        await table.UpsertEntityAsync(entity);
    }
}