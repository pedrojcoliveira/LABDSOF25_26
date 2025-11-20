using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class ListaPacientesPage : ContentPage
{
    private readonly ListaPacientesViewModel _viewModel;

    public ListaPacientesPage(ListaPacientesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carregar pacientes ao aparecer
        await _viewModel.CarregarPacientesCommand.ExecuteAsync(null);
    }

    private void OnPesquisaTextChanged(object sender, TextChangedEventArgs e)
    {
        // Executar pesquisa quando o texto muda
        _viewModel.PesquisarPacientesCommand.Execute(null);
    }
}