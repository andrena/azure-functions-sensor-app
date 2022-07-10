using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests.Functions;

[TestClass]
public class SensorInputFunctionTests
{
    private static readonly SensorInput SensorInput = new()
    {
        SensorBoxId = "test",
        Timestamp = DateTimeOffset.UtcNow,
        Data = new List<SensorData>
        {
            new()
            {
                Type = SensorType.Temperature,
                Values = new List<double> { 1, 2, 3 }
            }
        }
    };
        
    [TestMethod]
    public async Task CreatesOutputForEachAggregation()
    {
        var aggregatedSensorData = new List<AggregatedSensorData> {new(), new()};

        var sensorInputService = A.Fake<ISensorInputService>();
        A.CallTo(() => sensorInputService.ProcessInputAsync(SensorInput)).Returns(aggregatedSensorData);

        var function = new SensorInputFunction(sensorInputService, A.Fake<ILogger<SensorInputFunction>>());

        var outputCollector = A.Fake<IAsyncCollector<AggregatedSensorData>>();
            
        var result = await function.Run(SensorInput,
            outputCollector, 
            CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => outputCollector.AddAsync(A<AggregatedSensorData>._, A<CancellationToken>._)).MustHaveHappened(aggregatedSensorData.Count, Times.Exactly);
        A.CallTo(() => outputCollector.FlushAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}