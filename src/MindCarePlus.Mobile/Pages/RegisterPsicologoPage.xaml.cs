using AndroidX.Lifecycle;
using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class RegisterPsicologoPage : ContentPage
{
	public RegisterPsicologoPage(RegisterPsicologoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}