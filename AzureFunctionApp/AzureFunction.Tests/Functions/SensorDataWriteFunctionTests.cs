using System;
using System.Threading.Tasks;
using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests.Functions;

[TestClass]
public class SensorDataWriteFunctionTests
{
    [TestMethod]
    public async Task WritesAggregatedData()
    {
        var writeService = A.Fake<ISensorDataService>();
        var function = new SensorDataWriteFunction(writeService, A.Fake<ILogger<SensorDataWriteFunction>>());

        var aggregatedSensorData = new AggregatedSensorData
        {
            SensorBoxId = "test",
            SensorType = SensorType.Temperature,
            AggregationType = AggregationType.Mean,
            CreatedAt = DateTimeOffset.UtcNow,
            Value = 1
        };
        await function.Run(aggregatedSensorData);

        A.CallTo(() => writeService.InsertAsync(aggregatedSensorData)).MustHaveHappened();
    }
}