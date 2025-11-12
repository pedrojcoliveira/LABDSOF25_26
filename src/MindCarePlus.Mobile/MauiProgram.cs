using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Pages;

namespace MindCarePlus.Mobile;

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

        builder.Services.AddSingleton(sp =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var httpClient = new HttpClient(handler);

#if ANDROID
            httpClient.BaseAddress = new Uri("https://10.0.2.2:7291/");
#else
            httpClient.BaseAddress = new Uri("https://localhost:7291/");
#endif

            return httpClient;
        });

        // ViewModels
        builder.Services.AddTransient<MainPageModelView>();
        builder.Services.AddTransient<DashboardViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DashboardPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}