using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AzureFunction.Core.Models;
using AzureFunction.Core.Repositories;
using Microsoft.Extensions.Configuration;

namespace AzureFunction.Start
{
    class Program
    {
        private const int SensorBoxCount = 10;
        private const int SensorInputCount = 1;
        
        private static readonly IDictionary<SensorType, (double Min, double Max)> SensorRanges = new Dictionary<SensorType, (double Min, double Max)>
        {
            {SensorType.Temperature, (Min: -60, Max: 80)},
            {SensorType.Humidity, (Min: 0, Max: 100)},
            {SensorType.Pressure, (Min: 800, Max: 1200)},
            {SensorType.Quality, (Min: 0, Max: 500)}
        };

        static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false)
                .Build();

            var defaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            var random = new Random();
            var sensorRepository = new SensorRepository(configuration["AzureWebJobsStorage"], "sensors");
            var sensors = (await sensorRepository.GetAllAsync()).ToList();
            var sensorBoxes = sensors.GroupBy(x => x.BoxId).Select(g => new SensorBox {Id = g.Key}).ToList();
            while(sensorBoxes.Count < SensorBoxCount)
            {
                var boxId = Guid.NewGuid().ToString();
                foreach (var (sensorType, (min, max)) in SensorRanges)
                {
                    var sensor = new Sensor(boxId, sensorType)
                    {
                        Min = min,
                        Max = max
                    };
                    await sensorRepository.InsertAsync(sensor);
                    sensors.Add(sensor);   
                }
                sensorBoxes.Add(new SensorBox {Id = boxId});
            }

            var utcNow = DateTimeOffset.UtcNow;
            foreach (var sensorBox in sensorBoxes)
            {
                sensorBox.LastSend = utcNow.Subtract(TimeSpan.FromMinutes(1.0 + random.NextDouble()));
            }
            
            Console.WriteLine("All sensors are ready. Starting sending of inputs ...");
            
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            var uri = new Uri($"{configuration["ApplicationUri"]}api/SensorInput");
            var tasks = new List<Task<HttpResponseMessage>>();
            var taskChunk = new List<Task<HttpResponseMessage>>();
            var sentCount = 0;
            while (sentCount < SensorInputCount)
            {
                var timeWindow = TimeSpan.FromMinutes(1.0 + random.NextDouble());
                utcNow = DateTimeOffset.UtcNow;
                var oldSensorBoxes = sensorBoxes.Where(x => x.LastSend < utcNow - timeWindow).ToList();
                if (!oldSensorBoxes.Any())
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    continue;
                }
                var sensorBox = oldSensorBoxes.ElementAt(random.Next(0, oldSensorBoxes.Count));
                var sensorInput = new SensorInput
                {
                    SensorBoxId = sensorBox.Id,
                    Timestamp = utcNow,
                    Data = SensorRanges
                        .Select(x => new SensorData
                        {
                            Type = x.Key,
                            Values = Enumerable.Range(0, (int) ((utcNow - sensorBox.LastSend).TotalSeconds / 3.0)).Select(_ => CreateSensorValue(x.Key, random)).ToList()
                        })
                        .ToList()
                };
                sensorBox.LastSend = utcNow;
                var content = new StringContent(JsonSerializer.Serialize(sensorInput, defaultJsonOptions), Encoding.UTF8, "application/json");
                var task = httpClient.PostAsync(uri, content);
                tasks.Add(task);
                taskChunk.Add(task);
                sentCount++;
                
                if (taskChunk.Count >= 10)
                {
                    await Task.WhenAll(taskChunk);
                    taskChunk.Clear();
                    Console.WriteLine($"Sent {sentCount} inputs so far ...");
                }
            }

            var responses = await Task.WhenAll(tasks);
            
            Console.WriteLine($"Sent {tasks.Count()} sensor inputs for {sensorBoxes.Count} sensors boxes. {responses.Count(r => r.IsSuccessStatusCode)} tasks completed successfully, {responses.Count(r => !r.IsSuccessStatusCode)} tasks failed.");
            Console.ReadKey();
        }

        private static double CreateSensorValue(SensorType sensorType, Random random)
        {
            var x = random.NextDouble();
            var (min, max) = SensorRanges[sensorType];
            return min + x * (max - min);
        }
    }

    internal class SensorBox
    {
        internal string Id { get; set; }
        
        internal DateTimeOffset LastSend { get; set; }
    }
}