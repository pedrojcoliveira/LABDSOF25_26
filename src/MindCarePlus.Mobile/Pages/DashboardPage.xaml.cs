using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages
{
    [QueryProperty(nameof(PacienteId), "pacienteId")]
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;
        private string _pacienteId;

        public string PacienteId
        {
            get => _pacienteId;
            set
            {
                _pacienteId = value;
                if (!string.IsNullOrEmpty(value) && _viewModel != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _viewModel.LoadDashboardAsync(Guid.Parse(value));
                    });
                }
            }
        }

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // TODO: Remover quando login estiver implementado
            // Fallback para testes - usa ID do Paciente de Teste da BD
            if (string.IsNullOrEmpty(_pacienteId))
            {
                await _viewModel.LoadDashboardAsync(Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"));
            }
        }
    }
}