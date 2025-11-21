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
            Console.WriteLine("========================================");
            Console.WriteLine("🔍 DashboardPage.OnAppearing()");
            Console.WriteLine("========================================");

            // SEMPRE carrega os dados quando a página aparece
            await _viewModel.LoadDashboardAsync();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            Console.WriteLine("========================================");
            Console.WriteLine("🔍 DashboardPage.OnNavigatedTo()");
            Console.WriteLine("🔄 Forçando refresh...");
            Console.WriteLine("========================================");
        }

        private async void OnBadgeTapped(object sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is BadgeConquistadaDto badge)
            {
                await border.ScaleTo(0.9, 50);
                await border.ScaleTo(1.0, 50);

                await DisplayAlert(
                    $"{badge.ImagemUrl} {badge.Nome}",
                    $"{badge.Descricao}\n\nNível: {badge.Nivel}\n✨ Conquistada!",
                    "Fechar"
                );
            }
        }
    }
}