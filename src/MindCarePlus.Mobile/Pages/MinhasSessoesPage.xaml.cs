// Mobile/Pages/MinhasSessoesPage.xaml.cs
using MindCarePlus.Mobile.ViewModels;

namespace MindCarePlus.Mobile.Pages
{
    public partial class MinhasSessoesPage : ContentPage
    {
        public MinhasSessoesPage(MinhasSessoesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MinhasSessoesViewModel viewModel)
            {
                await viewModel.CarregarSessoesAsync();
            }
        }
    }
}