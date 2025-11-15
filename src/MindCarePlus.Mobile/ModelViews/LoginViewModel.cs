using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Auth;

namespace MindCarePlus.Mobile.ModelViews;
public partial class LoginViewModel: ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool showPassword;

    [ObservableProperty]
    private bool rememberMe;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoadSavedCredentials();
    }

    [RelayCommand]
    private async Task Login()
    {
        if (IsLoading) return;

        ErrorMessage = string.Empty;

        // Validações
        if (!ValidateInputs())
            return;

        IsLoading = true;

        try
        {
            var dto = new LoginDto
            {
                Email = Email.Trim(),
                Password = Password
            };

            var result = await _authService.LoginAsync(dto);

            if (result.Success)
            {
                // Guardar credenciais se "Lembrar-me" estiver ativo
                if (RememberMe)
                {
                    await SaveCredentials();
                }
                else
                {
                    ClearSavedCredentials();
                }

                // Navegar para a página principal
                await Shell.Current.GoToAsync("///MainPage");

                // Opcional: Mostrar mensagem de sucesso
                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    "Login efetuado com sucesso!",
                    "OK");
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
    private async Task NavigateToRegister()
    {
        await Shell.Current.GoToAsync("///SelectUserTypePage");
    }

    [RelayCommand]
    private async Task ForgotPassword()
    {
        // TODO: Implementar recuperação de password
        await Shell.Current.DisplayAlert(
            "Recuperar Password",
            "Funcionalidade em desenvolvimento",
            "OK");
    }

    private bool ValidateInputs()
    {
        // Validar email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor, insira o seu email";
            return false;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Por favor, insira um email válido";
            return false;
        }

        // Validar password
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, insira a sua password";
            return false;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "A password deve ter no mínimo 6 caracteres";
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    private async void LoadSavedCredentials()
    {
        try
        {
            var savedEmail = await SecureStorage.GetAsync("saved_email");
            var savedRememberMe = await SecureStorage.GetAsync("remember_me");

            if (!string.IsNullOrEmpty(savedEmail))
            {
                Email = savedEmail;
                RememberMe = savedRememberMe == "true";
            }
        }
        catch
        {
            // Ignorar erros ao carregar credenciais
        }
    }

    private async Task SaveCredentials()
    {
        try
        {
            await SecureStorage.SetAsync("saved_email", Email);
            await SecureStorage.SetAsync("remember_me", "true");
        }
        catch
        {
            // Ignorar erros ao guardar credenciais
        }
    }

    private void ClearSavedCredentials()
    {
        try
        {
            SecureStorage.Remove("saved_email");
            SecureStorage.Remove("remember_me");
        }
        catch
        {
            // Ignorar erros ao limpar credenciais
        }
    }
}
