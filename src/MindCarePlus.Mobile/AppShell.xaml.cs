namespace MindCarePlus.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("evolucao", typeof(Pages.EvolucaoPage));
    }
}
