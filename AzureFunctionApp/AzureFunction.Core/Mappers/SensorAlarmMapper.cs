using System;
using Azure.Data.Tables;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorAlarmMapper
{
    public static SensorAlarm Map(TableEntity entity)
    {
        return new SensorAlarm
        {
            SensorBoxId = entity.PartitionKey,
            Identifier = entity.RowKey,
            SensorType = Enum.Parse<SensorType>(entity.GetString(nameof(SensorAlarm.SensorType))),
            Status = Enum.Parse<AlarmStatus>(entity.GetString(nameof(SensorAlarm.Status))),
            FiredAt = entity.Timestamp ?? DateTimeOffset.MinValue
        };
    }

    public static TableEntity Map(SensorAlarm alarm)
    {
        return new TableEntity(alarm.SensorBoxId, alarm.Identifier)
        {
            [nameof(SensorAlarm.SensorType)] = alarm.SensorType.ToString("G"),
            [nameof(SensorAlarm.Status)] = alarm.Status.ToString("G"),
            Timestamp = alarm.FiredAt
        };
    }
}