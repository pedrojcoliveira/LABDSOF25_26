using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Carrega a dashboard automaticamente quando a página aparece
            await _viewModel.LoadDashboardAsync();
        }
    }
}