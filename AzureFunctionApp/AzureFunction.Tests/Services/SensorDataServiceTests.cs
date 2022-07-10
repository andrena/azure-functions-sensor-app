using System;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests.Services;

[TestClass]
public class SensorDataServiceTests
{
    [TestMethod]
    public async Task InsertAsyncTest()
    {
        var sensorDataRepository = A.Fake<ISensorDataRepository>();
        var service = new SensorDataService(sensorDataRepository);
        var aggregatedSensorData = new AggregatedSensorData
        {
            SensorBoxId = "test",
            SensorType = SensorType.Temperature,
            AggregationType = AggregationType.Mean,
            CreatedAt = DateTimeOffset.UtcNow,
            Value = 1
        };

        await service.InsertAsync(aggregatedSensorData);
            
        A.CallTo(() => sensorDataRepository.InsertAsync(aggregatedSensorData)).MustHaveHappened();
    }
        
}