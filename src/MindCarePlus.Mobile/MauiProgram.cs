using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Pages;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MindCarePlus.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(sp =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var httpClient = new HttpClient(handler);

#if ANDROID
            // Para Android físico, usa o Dev Tunnel
            httpClient.BaseAddress = new Uri("https://jolly-horse-kl34c8b-5144.uks1.devtunnels.ms/");
#else
            // Windows
            httpClient.BaseAddress = new Uri("https://localhost:7291/");
#endif

            return httpClient;
        });

        // ViewModels
        builder.Services.AddTransient<MainPageModelView>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<EvolucaoModelView>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<EvolucaoPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}