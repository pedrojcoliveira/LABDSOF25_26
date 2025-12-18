using MindCarePlus.Mobile.ViewModels;

namespace MindCarePlus.Mobile.Pages
{
    public partial class SolicitarSessaoPage : ContentPage
    {
        public SolicitarSessaoPage(SolicitarSessaoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}