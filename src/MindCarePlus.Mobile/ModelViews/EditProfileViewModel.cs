using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Mobile.ModelViews;

public partial class EditProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public EditProfileViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _idadeText = string.Empty;

    [ObservableProperty]
    private string? _objectivo;

    [ObservableProperty]
    private string? _especialidade;

    [ObservableProperty]
    private string _tipoUtilizador = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Indica se é Paciente
    /// </summary>
    public bool IsPaciente => TipoUtilizador == "Paciente";

    /// <summary>
    /// Indica se é Psicólogo
    /// </summary>
    public bool IsPsicologo => TipoUtilizador == "Psicologo";

    /// <summary>
    /// Indica se há erro
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Inverso de IsSaving para habilitar/desabilitar botão
    /// </summary>
    public bool IsNotSaving => !IsSaving;

    /// <summary>
    /// Iniciais do nome para o avatar
    /// </summary>
    public string Initials
    {
        get
        {
            if (string.IsNullOrEmpty(Nome)) return "?";
            var parts = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return Nome.Length >= 2 ? Nome[..2].ToUpper() : Nome.ToUpper();
        }
    }

    partial void OnNomeChanged(string value)
    {
        OnPropertyChanged(nameof(Initials));
    }

    partial void OnTipoUtilizadorChanged(string value)
    {
        OnPropertyChanged(nameof(IsPaciente));
        OnPropertyChanged(nameof(IsPsicologo));
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotSaving));
    }

    /// <summary>
    /// Carregar dados do perfil atual
    /// </summary>
    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var profile = await _apiService.GetMyProfileAsync();

            if (profile != null)
            {
                Nome = profile.Nome;
                Email = profile.Email;
                IdadeText = profile.Idade.ToString();
                TipoUtilizador = profile.TipoUtilizador;
                Objectivo = profile.Objectivo;
                Especialidade = profile.Especialidade;
            }
            else
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
    /// Guardar alterações do perfil
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsSaving) return;

        // Validações
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Nome))
        {
            ErrorMessage = "O nome é obrigatório";
            return;
        }

        if (Nome.Length < 2)
        {
            ErrorMessage = "O nome deve ter pelo menos 2 caracteres";
            return;
        }

        if (!int.TryParse(IdadeText, out int idade))
        {
            ErrorMessage = "A idade deve ser um número válido";
            return;
        }

        if (idade < 13 || idade > 120)
        {
            ErrorMessage = "A idade deve estar entre 13 e 120 anos";
            return;
        }

        try
        {
            IsSaving = true;

            var dto = new UpdateProfileDto
            {
                Nome = Nome.Trim(),
                Idade = idade,
                Objectivo = IsPaciente ? Objectivo?.Trim() : null,
                Especialidade = IsPsicologo ? Especialidade?.Trim() : null
            };

            var success = await _apiService.UpdateMyProfileAsync(dto);

            if (success)
            {
                await Shell.Current.DisplayAlert(
                    "Sucesso ✅",
                    "Perfil atualizado com sucesso!",
                    "OK");

                // Voltar para a página de perfil
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "Erro ao guardar alterações. Tente novamente.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Cancelar edição e voltar
    /// </summary>
    [RelayCommand]
    public async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}





