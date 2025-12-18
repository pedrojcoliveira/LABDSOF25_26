using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class DashboardPsicologoPage : ContentPage
{
    private readonly DashboardPsicologoViewModel _viewModel;

    public DashboardPsicologoPage(DashboardPsicologoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine("📄 DashboardPsicologoPage.OnAppearing - A carregar o dashboard...");
        await _viewModel.RefreshCommand.ExecuteAsync(null);
    }
}