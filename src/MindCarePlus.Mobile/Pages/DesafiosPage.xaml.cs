using MindCarePlus.Mobile.ViewModels;

namespace MindCarePlus.Mobile.Pages;

public partial class DesafiosPage : ContentPage
{
	public DesafiosPage(DesafiosModelView viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}