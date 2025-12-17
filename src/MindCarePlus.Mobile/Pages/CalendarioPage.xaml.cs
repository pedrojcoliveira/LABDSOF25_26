using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;


public partial class CalendarioPage : ContentPage
{
    private readonly CalendarioViewModel _viewModel;

    public CalendarioPage(CalendarioViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarSessoesCommand.ExecuteAsync(null);
    }
}