using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Views;

public partial class RelatoriosIAPage : ContentPage
{
    private readonly RelatorioIAModelView _viewModel;

    public RelatoriosIAPage(RelatorioIAModelView viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRelatoriosAsync();
    }
}