using MindCarePlus.Mobile.Pages;

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
        Routing.RegisterRoute("evolucao", typeof(Pages.EvolucaoPage));
        //Routing.RegisterRoute("Dashboard", typeof(DashboardPage));

        Routing.RegisterRoute("DetalhePacientePage", typeof(DetalhePacientePage));
    }
}
