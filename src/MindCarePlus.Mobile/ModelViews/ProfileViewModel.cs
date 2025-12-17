using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Mobile.ModelViews;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private int idade;

    [ObservableProperty]
    private string? objectivo;

    [ObservableProperty]
    private string? especialidade;

    [ObservableProperty]
    private int? nivelGamificacao;

    [ObservableProperty]
    private int? pontos;

    [ObservableProperty]
    private string tipoUtilizador = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isPaciente;

    [ObservableProperty]
    private bool isPsicologo;

    private Guid _userId;

    public ProfileViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoadProfile()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var profile = await _apiService.GetMyProfileAsync();

            if (profile != null)
            {
                _userId = profile.Id;
                Nome = profile.Nome;
                Email = profile.Email;
                Idade = profile.Idade;
                TipoUtilizador = profile.TipoUtilizador;

                IsPaciente = profile.TipoUtilizador == "Paciente";
                IsPsicologo = profile.TipoUtilizador == "Psicologo";

                if (IsPaciente)
                {
                    Objectivo = profile.Objectivo;
                    NivelGamificacao = profile.NivelGamificacao;
                    Pontos = profile.Pontos;
                }
                else if (IsPsicologo)
                {
                    Especialidade = profile.Especialidade;
                }
            }
            else
            {
                ErrorMessage = "Erro ao carregar perfil";
                await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar perfil: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        if (IsLoading) return;

        // Validações
        if (string.IsNullOrWhiteSpace(Nome))
        {
            ErrorMessage = "O nome é obrigatório";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
            return;
        }

        if (Idade < 13 || Idade > 120)
        {
            ErrorMessage = "A idade deve estar entre 13 e 120 anos";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var dto = new UpdateProfileDto
            {
                Nome = Nome.Trim(),
                Idade = Idade,
                Objectivo = IsPaciente ? Objectivo : null,
                Especialidade = IsPsicologo ? Especialidade : null
            };

            var success = await _apiService.UpdateMyProfileAsync(dto);

            if (success)
            {
                await Shell.Current.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
                IsEditing = false;

                // Recarregar perfil
                await LoadProfile();
            }
            else
            {
                ErrorMessage = "Erro ao atualizar perfil";
                await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao atualizar perfil: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        // Recarregar dados originais
        _ = LoadProfile();
    }

    [RelayCommand]
    private async Task Logout()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Confirmar Logout",
            "Tem a certeza que deseja sair?",
            "Sim",
            "Não");

        if (!confirm) return;

        IsLoading = true;

        try
        {
            var success = await _authService.LogoutAsync();

            if (success)
            {
                // Navegar para a página de login
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erro", "Erro ao fazer logout", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao fazer logout: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}



