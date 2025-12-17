using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Mobile.ModelViews;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    public ProfileViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    [ObservableProperty]
    private ProfileDto? _profile;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDeleting;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _deletePassword = string.Empty;

    [ObservableProperty]
    private bool _showDeleteConfirmation;

    /// <summary>
    /// Indica se o utilizador é Paciente
    /// </summary>
    public bool IsPaciente => Profile?.TipoUtilizador == "Paciente";

    /// <summary>
    /// Indica se o utilizador é Psicólogo
    /// </summary>
    public bool IsPsicologo => Profile?.TipoUtilizador == "Psicologo";

    /// <summary>
    /// Iniciais do nome para o avatar
    /// </summary>
    public string Initials
    {
        get
        {
            if (Profile == null || string.IsNullOrEmpty(Profile.Nome)) return "?";
            var parts = Profile.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return Profile.Nome.Length >= 2 ? Profile.Nome[..2].ToUpper() : Profile.Nome.ToUpper();
        }
    }

    /// <summary>
    /// Notificar mudanças quando Profile muda
    /// </summary>
    partial void OnProfileChanged(ProfileDto? value)
    {
        OnPropertyChanged(nameof(IsPaciente));
        OnPropertyChanged(nameof(IsPsicologo));
        OnPropertyChanged(nameof(Initials));
    }

    /// <summary>
    /// Carregar perfil do utilizador
    /// </summary>
    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Profile = await _apiService.GetMyProfileAsync();

            if (Profile == null)
            {
                ErrorMessage = "Não foi possível carregar o perfil";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navegar para página de edição de perfil
    /// User Story: UM.P.03 e UM.PS.19
    /// </summary>
    [RelayCommand]
    public async Task EditProfileAsync()
    {
        await Shell.Current.GoToAsync("EditProfilePage");
    }

    /// <summary>
    /// Mostrar diálogo de confirmação de eliminação
    /// User Story: UM.P.05 e UM.PS.20
    /// </summary>
    [RelayCommand]
    public void ShowDeleteConfirmationDialog()
    {
        ShowDeleteConfirmation = true;
        DeletePassword = string.Empty;
    }

    /// <summary>
    /// Cancelar eliminação
    /// </summary>
    [RelayCommand]
    public void CancelDelete()
    {
        ShowDeleteConfirmation = false;
        DeletePassword = string.Empty;
    }

    /// <summary>
    /// Confirmar e executar eliminação de conta
    /// User Story: UM.P.05 e UM.PS.20
    /// </summary>
    [RelayCommand]
    public async Task ConfirmDeleteAccountAsync()
    {
        if (string.IsNullOrEmpty(DeletePassword))
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                "Por favor, introduza a sua password para confirmar a eliminação.",
                "OK");
            return;
        }

        // Confirmação adicional
        var confirm = await Shell.Current.DisplayAlert(
            "⚠️ Confirmação Final",
            "Esta ação é IRREVERSÍVEL!\n\n" +
            "Todos os seus dados serão permanentemente eliminados:\n" +
            "• Perfil e informações pessoais\n" +
            "• Registos diários de humor\n" +
            "• Mensagens e conversas\n" +
            "• Sessões e avaliações\n" +
            "• Desafios e conquistas\n\n" +
            "Tem a certeza que deseja continuar?",
            "Sim, eliminar tudo",
            "Cancelar");

        if (!confirm)
        {
            return;
        }

        try
        {
            IsDeleting = true;
            ErrorMessage = string.Empty;

            var result = await _apiService.DeleteMyAccountAsync(DeletePassword);

            if (result == null)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    "Erro de comunicação com o servidor.",
                    "OK");
                return;
            }

            if (result.Success)
            {
                ShowDeleteConfirmation = false;
                await _authService.LogoutAsync();

                await Shell.Current.DisplayAlert(
                    "Conta Eliminada",
                    "A sua conta e todos os dados associados foram permanentemente eliminados.",
                    "OK");

                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    result.Message,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Erro ao eliminar conta: {ex.Message}",
                "OK");
        }
        finally
        {
            IsDeleting = false;
            DeletePassword = string.Empty;
        }
    }

    /// <summary>
    /// Logout do utilizador
    /// User Story: UM.P.07 e UM.PS.18
    /// </summary>
    [RelayCommand]
    public async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Logout",
            "Tem a certeza que deseja sair?",
            "Sim",
            "Não");

        if (confirm)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}



