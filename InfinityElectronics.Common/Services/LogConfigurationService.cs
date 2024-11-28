using Serilog;

namespace InfinityElectronics.Common.Services
{
    public static class LogConfigurationService
    {
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() 
                .WriteTo.Console() 
                .WriteTo.File("logs/application.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}