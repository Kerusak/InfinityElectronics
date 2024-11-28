using InfinityElectronics.Common.Services;
using InfinityElectronics.Common.Services.Interfaces;
using InfinityElectronics.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

LogConfigurationService.ConfigureLogging();

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        //var redisConnectionString = Environment.GetEnvironmentVariable("RedisConnectionString");
        //services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddMemoryCache();

        services.AddHttpClient();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection")));
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging(builder =>
        {
            builder.AddSerilog(); 
        });
        services.AddScoped<ICacheService, MemoryCacheService>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

host.Run();