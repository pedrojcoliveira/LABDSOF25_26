using MindCarePlus.Mobile.ModelViews;


namespace MindCarePlus.Mobile.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}