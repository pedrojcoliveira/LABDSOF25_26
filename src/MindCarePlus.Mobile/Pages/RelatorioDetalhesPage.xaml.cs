using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Views;

[QueryProperty(nameof(RelatorioId), "relatorioId")]
public partial class RelatorioDetalhesPage : ContentPage
{
    private readonly RelatorioDetalhesViewModel _viewModel;

    public RelatorioDetalhesPage(RelatorioDetalhesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private string _relatorioId;
    public string RelatorioId
    {
        get => _relatorioId;
        set
        {
            _relatorioId = value;
            if (Guid.TryParse(value, out var guid))
            {
                Task.Run(async () => await _viewModel.LoadRelatorioAsync(guid));
            }
        }
    }
}