using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class SelectUserTypePage : ContentPage
{
	public SelectUserTypePage(SelectUserTypeViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}