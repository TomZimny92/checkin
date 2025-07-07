using Checkin.Services;
using Checkin.ViewModel;
using Microsoft.Extensions.Logging;

namespace Checkin;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<BaseViewModel>();
        builder.Services.AddSingleton<IDispatcherTimer>((timer) => Application.Current.Dispatcher.CreateTimer());
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();

        return builder.Build();
    }
}