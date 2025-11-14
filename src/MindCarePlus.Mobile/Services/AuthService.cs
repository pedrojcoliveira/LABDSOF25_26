using System.Net.Http.Json;
using MindCarePlus.Shared.DTOs.Auth;


namespace MindCarePlus.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000/api" // Android emulator
            : "http://localhost:5000/api"; // iOS simulator ou Windows
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


