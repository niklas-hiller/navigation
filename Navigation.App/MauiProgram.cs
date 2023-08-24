using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Navigation.App.Services;
using Navigation.App.ViewModels.Sensors;
using Navigation.App.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Navigation.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseSkiaSharp(true)
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureLifecycleEvents(events =>
            {
                var lifecycleService = Services.ServiceProvider.GetService<LifecycleService>();
#if ANDROID     
                events.AddAndroid(android => android
                    .OnCreate((activity, bundle) => lifecycleService.OnCreate())
                    .OnDestroy((activity) => lifecycleService.OnDestroy()));
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<InsightPage>();
        builder.Services.AddSingleton<LifecycleService>();
        builder.Services.AddSingleton<AccelerometerViewModel>();
        builder.Services.AddSingleton<SensorService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);

#if ANDROID
        builder.Services.AddTransient<IService, DemoServices>();
#endif

        return builder.Build();
    }
}
