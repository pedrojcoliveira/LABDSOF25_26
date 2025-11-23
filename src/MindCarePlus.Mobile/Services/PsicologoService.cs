using MindCarePlus.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.Services;

public class PsicologoService
{
    private readonly HttpClient _httpClient;

    public PsicologoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// Configura o header de autenticação com o token JWT
    private async Task ConfigureAuthHeaderAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }


    /// Trata erros de autenticação (redireciona para login)
    private async Task HandleUnauthorizedAsync()
    {
        SecureStorage.Remove("auth_token");
        await Shell.Current.DisplayAlert("Sessão Expirada", "Por favor, faça login novamente", "OK");
        await Shell.Current.GoToAsync("//login");
    }

    public async Task<List<PsicologoContactoDto>> ListarPsicologosAsync(
        string? especialidade = null,
        bool apenasDisponiveis = true)
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var url = $"api/Psicologos?apenasDisponiveis={apenasDisponiveis}";

            if (!string.IsNullOrWhiteSpace(especialidade))
            {
                url += $"&especialidade={Uri.EscapeDataString(especialidade)}";
            }

            Console.WriteLine($"🔍 A procurar psicólogos: {url}");
            Console.WriteLine($"🌐 BaseAddress: {_httpClient.BaseAddress}");

            var psicologos = await _httpClient.GetFromJsonAsync<List<PsicologoContactoDto>>(url);

            Console.WriteLine($"✅ {psicologos?.Count ?? 0} psicólogos encontrados");

            return psicologos ?? new List<PsicologoContactoDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Erro HTTP ao procurar psicólogos: {ex.Message}");
            Console.WriteLine($"❌ StatusCode: {ex.StatusCode}");

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }

            return new List<PsicologoContactoDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro inesperado ao procurar psicólogos: {ex.Message}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            return new List<PsicologoContactoDto>();
        }
    }

    public async Task<PsicologoContactoDto?> ObterPsicologoPorIdAsync(Guid psicologoId)
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            Console.WriteLine($"🔍 A procurar psicólogo {psicologoId}");

            var psicologo = await _httpClient.GetFromJsonAsync<PsicologoContactoDto>(
                $"api/Psicologos/{psicologoId}");

            Console.WriteLine($"✅ Psicólogo encontrado: {psicologo?.Nome}");

            return psicologo;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Erro ao procurar psicólogo: {ex.Message}");

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("⚠️ Psicólogo não encontrado");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro inesperado: {ex.Message}");
            return null;
        }
    }
}