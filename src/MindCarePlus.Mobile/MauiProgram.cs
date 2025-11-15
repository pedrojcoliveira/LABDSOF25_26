using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Mobile.Pages;
using MindCarePlus.Mobile.Services;

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

        builder.Services.AddSingleton<HttpClient>();

        // Registar Services
        builder.Services.AddSingleton<AuthService>();

        // Registar ViewModels
        builder.Services.AddTransient<SelectUserTypeViewModel>();
        builder.Services.AddTransient<RegisterPacienteViewModel>();
        builder.Services.AddTransient<RegisterPsicologoViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainPageModelView>();

        // Registar Pages
        builder.Services.AddTransient<SelectUserTypePage>();
        builder.Services.AddTransient<RegisterPacientePage>();
        builder.Services.AddTransient<RegisterPsicologoPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
