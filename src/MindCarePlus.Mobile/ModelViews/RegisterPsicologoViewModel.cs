using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Auth;

namespace MindCarePlus.Mobile.ModelViews;
public partial class RegisterPsicologoViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string idade = string.Empty;

    [ObservableProperty]
    private string numeroCedulaProfissional = string.Empty;

    [ObservableProperty]
    private string especialidade = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool showPassword;

    [ObservableProperty]
    private bool showConfirmPassword;

    public RegisterPsicologoViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Register()
    {
        if (IsLoading) return;

        ErrorMessage = string.Empty;

        // Validações
        if (!ValidateInputs())
            return;

        IsLoading = true;

        try
        {
            var dto = new RegisterPsicologoDto
            {
                Nome = Nome.Trim(),
                Email = Email.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                Idade = int.Parse(Idade),
                NumeroCedulaProfissional = NumeroCedulaProfissional.Trim(),
                Especialidade = Especialidade.Trim()
            };

            var result = await _authService.RegisterPsicologoAsync(dto);

            if (result.Success)
            {
                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    result.Message,
                    "OK");

                await Shell.Current.GoToAsync("///LoginPage");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert(
                    "Erro",
                    result.Message,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            await Shell.Current.DisplayAlert(
                "Erro",
                ErrorMessage,
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleShowPassword()
    {
        ShowPassword = !ShowPassword;
    }

    [RelayCommand]
    private void ToggleShowConfirmPassword()
    {
        ShowConfirmPassword = !ShowConfirmPassword;
    }

    [RelayCommand]
    private async Task NavigateBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private bool ValidateInputs()
    {
        // Validar Nome
        if (string.IsNullOrWhiteSpace(Nome))
        {
            ErrorMessage = "O nome é obrigatório";
            return false;
        }

        if (Nome.Length < 2 || Nome.Length > 100)
        {
            ErrorMessage = "O nome deve ter entre 2 e 100 caracteres";
            return false;
        }

        // Validar Email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "O email é obrigatório";
            return false;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Email inválido";
            return false;
        }

        // Validar Password
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "A palavra-passe é obrigatória";
            return false;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "A palavra-passe deve ter no mínimo 6 caracteres";
            return false;
        }

        // Validar ConfirmPassword
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "As palavras-passe não coincidem";
            return false;
        }

        // Validar Idade
        if (string.IsNullOrWhiteSpace(Idade))
        {
            ErrorMessage = "A idade é obrigatória";
            return false;
        }

        if (!int.TryParse(Idade, out int idadeValue) || idadeValue < 13 || idadeValue > 120)
        {
            ErrorMessage = "A idade deve estar entre 13 e 120 anos";
            return false;
        }

        // Validar Número da Cédula Profissional
        if (string.IsNullOrWhiteSpace(NumeroCedulaProfissional))
        {
            ErrorMessage = "O número da cédula profissional é obrigatório";
            return false;
        }

        if (NumeroCedulaProfissional.Length > 50)
        {
            ErrorMessage = "O número da cédula profissional não pode exceder 50 caracteres";
            return false;
        }

        // Validar Especialidade
        if (string.IsNullOrWhiteSpace(Especialidade))
        {
            ErrorMessage = "A especialidade é obrigatória";
            return false;
        }

        if (Especialidade.Length > 100)
        {
            ErrorMessage = "A especialidade não pode exceder 100 caracteres";
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

}
