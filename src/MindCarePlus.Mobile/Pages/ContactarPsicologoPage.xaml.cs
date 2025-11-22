using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class ContactarPsicologoPage : ContentPage
{
    private readonly ContactarPsicologoViewModel _viewModel;

    public ContactarPsicologoPage(ContactarPsicologoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarPsicologosAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.PesquisarPsicologosCommand.Execute(null);
    }

    private async void OnEspecialidadeChanged(object sender, EventArgs e)
    {
        await _viewModel.FiltrarPorEspecialidadeAsync();
    }
}