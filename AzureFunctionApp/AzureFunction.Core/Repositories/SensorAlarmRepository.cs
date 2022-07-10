using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Repositories;

public class SensorAlarmRepository : ISensorAlarmRepository
{
    private readonly string _tableName;
    private readonly TableServiceClient _client;

    public SensorAlarmRepository(string connectionString, string tableName)
    {
        _tableName = tableName;
        _client = new TableServiceClient(connectionString);
    }

    public async Task<SensorAlarm?> GetBySensorBoxIdAndSensorTypeAndStatusAsync(string sensorBoxId, SensorType sensorType, AlarmStatus status)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        var sensorTypeString = sensorType.ToString("G");
        var alarmStatusString = status.ToString("G");
        var entityPages = table.QueryAsync<TableEntity>($"{nameof(TableEntity.PartitionKey)} eq '{sensorBoxId}' and {nameof(SensorAlarm.SensorType)} eq '{sensorTypeString}' and {nameof(SensorAlarm.Status)} eq '{alarmStatusString}'");
        return (await entityPages.AsPages().FirstOrDefaultAsync())?.Values.Select(SensorAlarmMapper.Map).FirstOrDefault();
    }

    public async Task InsertAsync(SensorAlarm alarm)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        await table.UpsertEntityAsync(SensorAlarmMapper.Map(alarm));
    }

    public async Task UpdateAsync(SensorAlarm alarm)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        await table.UpsertEntityAsync(SensorAlarmMapper.Map(alarm));
    }

    public async Task DeleteAsync(SensorAlarm alarm)
    {
        var table = _client.GetTableClient(_tableName);
        await table.CreateIfNotExistsAsync();
        await table.DeleteEntityAsync(alarm.SensorBoxId, alarm.Identifier);
    }
}