using System;

namespace AzureFunction.Core.Models;

public class SensorAlarm
{
    public SensorAlarm()
    {
        FiredAt = DateTimeOffset.UtcNow;
        Identifier = Guid.NewGuid().ToString();
    }

    public string SensorBoxId { get; set; } = null!;
        
    public string Identifier { get; set; }

    public SensorType SensorType { get; set; }

    public DateTimeOffset FiredAt { get; set; }

    public AlarmStatus Status { get; set; }
}

public enum AlarmStatus
{
    InvalidData, Dead, Closed
}