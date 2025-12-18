using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class DetalhesSessaoPage : ContentPage
{
    public DetalhesSessaoPage(DetalhesSessaoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}