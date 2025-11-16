using MindCarePlus.Shared.DTOs.Auth;
using System.Buffers.Text;
using System.Net.Http.Json;


namespace MindCarePlus.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://62h5zfnx-7291.uks1.devtunnels.ms/api" // Android emulator
            : "http://localhost:7291/api"; // iOS simulator ou Windows
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/auth/login",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                // Se login bem-sucedido, guardar o token
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.SetAsync("auth_token", result.Token);
                }

                return result ?? new AuthResponseDto
                {
                    Success = false,
                    Message = "Erro ao processar resposta"
                };
            }

            var errorContent = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return errorContent ?? new AuthResponseDto
            {
                Success = false,
                Message = "Credenciais inválidas"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }


    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("auth_token");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            // 1. Obter o token atual
            var token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                // Já não está logado
                return true;
            }

            // 2. Chamar a API (opcional, se quiseres registar o logout no servidor)
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{_baseUrl}/auth/logout", null);

            // 3. Limpar o token do SecureStorage (SEMPRE, mesmo se a API falhar)
            SecureStorage.Remove("auth_token");

            // 4. Limpar o header de autorização
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return true;
        }
        catch (Exception ex)
        {
            // Mesmo com erro, remove o token localmente
            SecureStorage.Remove("auth_token");

            System.Diagnostics.Debug.WriteLine($"Erro no logout: {ex.Message}");
            return true; // Retorna true porque o token foi removido localmente
        }
    }

    public async Task<AuthResponseDto> RegisterPacienteAsync(RegisterPacienteDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/auth/register/paciente",
                dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                    ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
            }

            var errorContent = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return errorContent ?? new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao registar paciente"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponseDto> RegisterPsicologoAsync(RegisterPsicologoDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/auth/register/psicologo",
                dto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                    ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
            }

            var errorContent = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return errorContent ?? new AuthResponseDto
            {
                Success = false,
                Message = "Erro ao registar psicólogo"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/auth/check-email/{Uri.EscapeDataString(email)}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}


