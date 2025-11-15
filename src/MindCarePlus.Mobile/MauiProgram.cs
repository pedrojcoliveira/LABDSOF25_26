using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Views;

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
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://mtnfgs8z-7291.uks1.devtunnels.ms/");
            return httpClient;
        });

        // ViewModels  
        builder.Services.AddTransient<MainPageModelView>();
        builder.Services.AddTransient<RelatorioIAModelView>();
        builder.Services.AddTransient<RelatorioDetalhesViewModel>();

        // Pages  
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<RelatoriosIAPage>();
        builder.Services.AddTransient<RelatorioDetalhesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}