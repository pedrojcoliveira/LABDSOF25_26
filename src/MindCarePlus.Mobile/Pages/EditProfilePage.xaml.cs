using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EditProfileViewModel vm)
        {
            await vm.LoadProfileAsync();
        }
    }
}