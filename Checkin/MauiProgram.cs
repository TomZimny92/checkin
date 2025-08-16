using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Serilog;
using Serilog.Sinks.Debug;


namespace Checkin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            IServiceCollection services = builder.Services;

            services.AddSerilog(
                new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(FileSystem.Current.AppDataDirectory, "MainViewModel-log.txt"), rollingInterval: RollingInterval.Month)
                    .CreateLogger());

            return builder.Build();
        }
    }
}
