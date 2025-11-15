using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Text.RegularExpressions;

using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Auth;


namespace MindCarePlus.Mobile.ModelViews;
public partial class RegisterPacienteViewModel : ObservableObject
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
    private string objectivo = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool showPassword;

    [ObservableProperty]
    private bool showConfirmPassword;

    public RegisterPacienteViewModel(AuthService authService)
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
            var dto = new RegisterPacienteDto
            {
                Nome = Nome.Trim(),
                Email = Email.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                Idade = int.Parse(Idade),
                Objectivo = string.IsNullOrWhiteSpace(Objectivo) ? null : Objectivo.Trim()
            };

            var result = await _authService.RegisterPacienteAsync(dto);

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

        // Validar Objectivo (opcional, mas tem limite)
        if (!string.IsNullOrWhiteSpace(Objectivo) && Objectivo.Length > 500)
        {
            ErrorMessage = "O objetivo não pode exceder 500 caracteres";
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
