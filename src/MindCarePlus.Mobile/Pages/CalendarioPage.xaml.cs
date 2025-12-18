using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Shared.DTOs;

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

    [RelayCommand]
    public async Task VerDetalhesSessaoAsync(SessaoCalendarioDto sessao)
    {
        if (sessao == null) return;

        // Alterado para cumprir UM.PS.08 com navegação mais rica
        var navigationParameter = new Dictionary<string, object>
        {
            { "Sessao", sessao }
        };

        await Shell.Current.GoToAsync("DetalhesSessaoPage", navigationParameter);
    }
}