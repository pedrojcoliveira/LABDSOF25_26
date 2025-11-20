using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Pages;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Mobile.Services.Interfaces;
using MindCarePlus.Mobile.ViewModels;
using Newtonsoft.Json.Linq;
using System.Reflection;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microcharts.Maui;

namespace MindCarePlus.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ===== 1. CARREGAR O APPCONFIG.JSON =====
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("MindCarePlus.Mobile.appconfig.json");

        // **Variável 'baseUrl' é declarada AQUI**
        string baseUrl;

        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var jObj = JObject.Parse(json);

            baseUrl = jObj["ApiBaseUrl"]?.ToString(); // Atribuída aqui
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("ApiBaseUrl não encontrado no appconfig.json");
            }

            builder.Configuration.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ApiBaseUrl", baseUrl) });
        }
        else
        {
            throw new FileNotFoundException("appconfig.json não encontrado. Definir como 'Embedded resource");
        }

        // ===== 2. SERVICES  =====
        // Registar o nosso handler de HTTP (para Android) num só sítio

        // **CORREÇÃO 1: Adicionado "sp =>"**
        builder.Services.AddSingleton<HttpClientHandler>(sp =>
        {
#if ANDROID
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
#else
            return new HttpClientHandler();
#endif
        });

        // **CORREÇÃO 2: Apagámos a segunda declaração de 'baseUrl'**
        // A variável 'baseUrl' da linha 36 já está disponível aqui.

        // Registar o ApiService
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl); // Usa a baseUrl da linha 36
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler<HttpClientHandler>(); // Usa o handler registado

        // Registar o AuthService (IGUAL AO APISERVICE)
        builder.Services.AddHttpClient<AuthService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl); // Usa a baseUrl da linha 36
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler<HttpClientHandler>(); // Usa o handler registado

        builder.Services.AddSingleton<IDialogService, MauiDialogService>();

        // Registar ViewModels
        builder.Services.AddTransient<SelectUserTypeViewModel>();
        builder.Services.AddTransient<RegisterPacienteViewModel>();
        builder.Services.AddTransient<RegisterPsicologoViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainPageModelView>();
        builder.Services.AddTransient<RegistoHumorModelView>();
        builder.Services.AddTransient<DesafiosModelView>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ListaPacientesViewModel>();
        builder.Services.AddTransient<DetalhePacienteViewModel>();
        builder.Services.AddTransient<ListaPacientesViewModel>();
        builder.Services.AddTransient<DetalhePacienteViewModel>();
        builder.Services.AddTransient<EvolucaoModelView>();

        // Registar Pages
        builder.Services.AddTransient<SelectUserTypePage>();
        builder.Services.AddTransient<RegisterPacientePage>();
        builder.Services.AddTransient<RegisterPsicologoPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<RegistoHumorPage>();
        builder.Services.AddTransient<DesafiosPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<EvolucaoPage>();
        builder.Services.AddTransient<ListaPacientesPage>();
        builder.Services.AddTransient<DetalhePacientePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}