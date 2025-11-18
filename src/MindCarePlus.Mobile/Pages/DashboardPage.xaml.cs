using MindCarePlus.Mobile.ModelViews;
using MindCarePlus.Shared.DTOs;

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

        private async void OnBadgeTapped(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is BadgeConquistadaDto badge)
            {
                // Animação de feedback visual
                await border.ScaleTo(0.9, 50);
                await border.ScaleTo(1.0, 50);

                // Mostrar popup com detalhes
                await DisplayAlert(
                    $"{badge.ImagemUrl} {badge.Nome}",
                    $"{badge.Descricao}✨ Conquistada com sucesso! Está no nivel: {badge.Nivel}\n\n",
                    "Fechar"
                );
            }
        }
    }
}