using MindCarePlus.Mobile.Pages;
using MindCarePlus.Mobile.Views;

namespace MindCarePlus.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registar rotas para navegação
        //Routing.RegisterRoute("SelectUserTypePage", typeof(SelectUserTypePage));
        Routing.RegisterRoute("RegisterPacientePage", typeof(RegisterPacientePage));
        Routing.RegisterRoute("RegisterPsicologoPage", typeof(RegisterPsicologoPage));
        Routing.RegisterRoute("RegistoHumor", typeof(RegistoHumorPage));
        Routing.RegisterRoute("DesafiosPage", typeof(DesafiosPage));
        Routing.RegisterRoute("evolucao", typeof(EvolucaoPage));
        //Routing.RegisterRoute("Dashboard", typeof(DashboardPage));
        Routing.RegisterRoute("ContactarPsicologoPage", typeof(ContactarPsicologoPage));
        Routing.RegisterRoute("ChatPage", typeof(ChatPage));
        Routing.RegisterRoute("DetalhePacientePage", typeof(DetalhePacientePage));
        Routing.RegisterRoute("relatoriodetalhes", typeof(RelatorioDetalhesPage));
        Routing.RegisterRoute("CriarTarefaPage", typeof(CriarTarefaPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));

    }


    private async void OnProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProfilePage");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Confirmar Logout",
            "Tem a certeza que deseja sair?",
            "Sim",
            "Não");

        if (!confirm) return;

        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("user_role");
        SecureStorage.Remove("user_email");


        await Shell.Current.GoToAsync("//LoginPage");

    }
}
