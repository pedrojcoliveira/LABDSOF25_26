using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Pages;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Mobile.ViewModels;
using Newtonsoft.Json.Linq;
using System.Reflection;

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

        // ===== 1. CARREGAR O APPCONFIG.JSON =====
        // (Certifica-te que o appconfig.json está como "Embedded resource")
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("MindCarePlus.Mobile.appconfig.json");

        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var jObj = JObject.Parse(json);

            var baseUrl = jObj["ApiBaseUrl"]?.ToString();
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("ApiBaseUrl não encontrado no appconfig.json");
            }

            builder.Configuration.AddInMemoryCollection(new[]{ new KeyValuePair<string,string>("ApiBaseUrl", baseUrl) });
        }
        else
        {
            throw new FileNotFoundException("appconfig.json não encontrado. Definir como 'Embedded resource");
        }

        // ===== 2. SERVICES  =====

        
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            // Lê o URL do appconfig.json
            string baseUrl = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("ApiBaseUrl não encontrado no appconfig.json");
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            // Movemos a lógica do Android para aqui
#if ANDROID
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
#else
            return new HttpClientHandler();
#endif
        });


        builder.Services.AddSingleton<IDialogService, MauiDialogService>();

        // ===== VIEWMODELS =====
        builder.Services.AddTransient<RegistoHumorModelView>();
        builder.Services.AddTransient<MainPageModelView>();
        

        // ===== PAGES =====
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<RegistoHumorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}