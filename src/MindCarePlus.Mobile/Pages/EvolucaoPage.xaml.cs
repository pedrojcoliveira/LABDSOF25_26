using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages
{
    [QueryProperty(nameof(PacienteId), "pacienteId")]
    public partial class EvolucaoPage : ContentPage
    {
        private readonly EvolucaoModelView _viewModel;
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
                        await _viewModel.LoadEvolucaoAsync(Guid.Parse(value));
                    });
                }
            }
        }

        public EvolucaoPage(EvolucaoModelView viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrEmpty(_pacienteId))
            {
                await _viewModel.LoadEvolucaoAsync(Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"));
            }
        }

        private async void OnRegistarHumorClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("RegistoHumor");
        }
    }
}