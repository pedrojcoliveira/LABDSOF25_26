using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class CriarTarefaPage : ContentPage
{
    public CriarTarefaPage(CriarTarefaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}