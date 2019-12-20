﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Models;
using AzureFunction.Core.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunction.Tests
{
    [TestClass]
    public class SensorInputServiceTests
    {
        [TestMethod]
        public async Task CreatesAllAggregations()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetById("test")).Returns(new Sensor {Id = "test", Type = SensorType.Temperature});
            var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

            var aggregatedSensorData = await service.ProcessInputAsync(new SensorInput {SensorId = "test", Values = new List<double> {1, 2, 3}});

            aggregatedSensorData.Should().HaveCount(4);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Mean);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Min);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.Max);
            aggregatedSensorData.Should().Contain(x => x.AggregationType == AggregationType.StandardDeviation);
        }

        [TestMethod]
        public async Task CalculatesAggregationsProperly()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetById("test")).Returns(new Sensor { Id = "test", Type = SensorType.Temperature });
            var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

            var aggregatedSensorData = await service.ProcessInputAsync(new SensorInput {SensorId = "test", Values = new List<double> {1, 2, 3}});

            aggregatedSensorData.First(a => a.AggregationType == AggregationType.Mean).Value.Should().Be(2d);
            aggregatedSensorData.First(a => a.AggregationType == AggregationType.Min).Value.Should().Be(1d);
            aggregatedSensorData.First(a => a.AggregationType == AggregationType.Max).Value.Should().Be(3d);
            aggregatedSensorData.First(a => a.AggregationType == AggregationType.StandardDeviation).Value.Should().Be(1d);
        }

        [TestMethod]
        public async Task ReturnsNoAggregationsIfInputIsEmpty()
        {
            var sensorRepository = A.Fake<ISensorRepository>();
            A.CallTo(() => sensorRepository.GetById("test")).Returns(new Sensor { Id = "test", Type = SensorType.Temperature });
            var service = new SensorInputService(sensorRepository, A.Fake<ILogger<SensorInputService>>());

            var aggregatedSensorData = await service.ProcessInputAsync(new SensorInput {SensorId = "test", Values = new List<double>()});

            aggregatedSensorData.Should().BeEmpty();
        }
    }
}