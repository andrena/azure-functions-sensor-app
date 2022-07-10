using AzureFunction.App;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Repositories;
using AzureFunction.Core.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunction.App;

public class Startup : FunctionsStartup, IWebJobsStartup2
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddLogging(c => c.AddConsole())
            .AddScoped<ISensorRepository>(p => 
                new SensorRepository(p.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"], "sensors"))
            .AddScoped<ISensorAlarmRepository>(p => 
                new SensorAlarmRepository(p.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"], "sensoralarms"))
            .AddScoped<ISensorDataRepository>(p => new SensorDataRepository(p.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"], "sensordata"))
            .AddScoped<ISensorInputService, SensorInputService>()
            .AddScoped<ISensorValidationService, SensorValidationService>()
            .AddScoped<ISensorDataService, SensorDataService>()
            .AddScoped<ISensorService, SensorService>();
    }

    void IWebJobsStartup2.Configure(WebJobsBuilderContext context, IWebJobsBuilder builder)
    {
        Configure(builder);
    }
}