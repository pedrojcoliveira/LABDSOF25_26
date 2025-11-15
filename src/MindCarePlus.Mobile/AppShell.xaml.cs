using MindCarePlus.Mobile.Views;

namespace MindCarePlus.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("relatoriodetalhes", typeof(RelatorioDetalhesPage));
    }
}
