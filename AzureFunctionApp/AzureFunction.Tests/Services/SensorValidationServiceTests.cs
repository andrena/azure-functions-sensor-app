using System;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests.Services;

[TestClass]
public class SensorValidationServiceTests
{
    [TestMethod]
    public async Task ValidateAggregatedDataAsyncTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetByBoxIdAndTypeAsync("test", SensorType.Temperature)).Returns(new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10) });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
        A.CallTo(() => sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatusAsync("test", SensorType.Temperature, AlarmStatus.InvalidData)).Returns((SensorAlarm?)null);

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.ValidateSensorDataAsync(new AggregatedSensorData { SensorBoxId = "test", AggregationType = AggregationType.Max, SensorType = SensorType.Temperature, Value = 4 });
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.InvalidData))).MustHaveHappened();

        await service.ValidateSensorDataAsync(new AggregatedSensorData { SensorBoxId = "test", AggregationType = AggregationType.Max, SensorType = SensorType.Temperature, Value = 11 });
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.InvalidData))).MustHaveHappened();
    }

    [TestMethod]
    public async Task CheckSensorsAndAlarmsTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetAllAsync()).Returns(new[] { new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10) } });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
        A.CallTo(() => sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatusAsync("test", SensorType.Temperature, AlarmStatus.Dead)).Returns((SensorAlarm?)null);

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
        A.CallTo(() => sensorAlarmRepository.InsertAsync(A<SensorAlarm>.That.Matches(a => a.Status == AlarmStatus.Dead))).MustHaveHappened();
    }

    [TestMethod]
    public async Task CheckObsoleteAlarmsTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetAllAsync()).Returns(new[] { new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow } });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
        var alarm = new SensorAlarm { SensorBoxId = "test", SensorType = SensorType.Temperature, Status = AlarmStatus.Dead };
        A.CallTo(() => sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatusAsync("test", SensorType.Temperature, AlarmStatus.Dead)).Returns(alarm);

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
        A.CallTo(() => sensorAlarmRepository.DeleteAsync(alarm)).MustHaveHappened();
    }
        
    [TestMethod]
    public async Task CheckObsoleteAlarmsResultsNullTest()
    {
        var sensorRepository = A.Fake<ISensorRepository>();
        A.CallTo(() => sensorRepository.GetAllAsync()).Returns(new[] { new Sensor("test", SensorType.Temperature) { Min = 5, Max = 10, LastSeen = DateTimeOffset.UtcNow } });
        var sensorAlarmRepository = A.Fake<ISensorAlarmRepository>();
        A.CallTo(() => sensorAlarmRepository.GetBySensorBoxIdAndSensorTypeAndStatusAsync("test", SensorType.Temperature, AlarmStatus.Dead)).Returns((SensorAlarm?)null);

        var service = new SensorValidationService(sensorRepository, sensorAlarmRepository, A.Fake<ILogger<SensorValidationService>>());

        await service.CheckSensorsAndAlarmsAsync();
            
        A.CallTo(() => sensorAlarmRepository.DeleteAsync(A<SensorAlarm>._)).MustNotHaveHappened();
    }
}