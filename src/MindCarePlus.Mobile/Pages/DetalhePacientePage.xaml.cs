using MindCarePlus.Mobile.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Pages;

public partial class DetalhePacientePage : ContentPage
{
    private readonly DetalhePacienteViewModel _viewModel;

    public DetalhePacientePage(DetalhePacienteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carregar detalhes do paciente ao aparecer
        await _viewModel.CarregarDetalhesCommand.ExecuteAsync(null);
    }
}