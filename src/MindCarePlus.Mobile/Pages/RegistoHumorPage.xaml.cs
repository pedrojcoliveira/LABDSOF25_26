using MindCarePlus.Mobile.ViewModels;

namespace MindCarePlus.Mobile.Pages
{
    public partial class RegistoHumorPage : ContentPage
    {
        public RegistoHumorPage(RegistoHumorModelView viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}