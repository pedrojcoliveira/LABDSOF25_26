using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class RegisterPacientePage : ContentPage
{
	public RegisterPacientePage(RegisterPacienteViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}