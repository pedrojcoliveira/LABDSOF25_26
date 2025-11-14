using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindCarePlus.Mobile.ModelViews;
public partial class SelectUserTypeViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToRegisterPaciente()
    {
        await Shell.Current.GoToAsync("///RegisterPacientePage");
    }

    [RelayCommand]
    private async Task NavigateToRegisterPsicologo()
    {
        await Shell.Current.GoToAsync("///RegisterPsicologoPage");
    }

    [RelayCommand]
    private async Task NavigateToLogin()
    {
        await Shell.Current.GoToAsync("///LoginPage");
    }
}
