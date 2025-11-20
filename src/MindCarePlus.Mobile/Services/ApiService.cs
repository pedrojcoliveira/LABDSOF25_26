using MindCarePlus.Shared.DTOs;
using MindCarePlus.Shared.DTOs.RegistoDiario;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;


        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

        }

        private async Task ConfigureAuthHeaderAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: Buscar registos por paciente
        public async Task<List<RegistoDiarioDto>> GetMyRegistosAsync()
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                var registos = await _httpClient.GetFromJsonAsync<List<RegistoDiarioDto>>("api/RegistoDiario/me");
                return registos ?? new List<RegistoDiarioDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao buscar registos: {ex.Message}");
                await HandleUnauthorizedAsync();
                return new List<RegistoDiarioDto>();
            }
        }

        // POST: Criar novo registo
        public async Task<RegistoDiarioDto?> CriarRegistoAsync(CriarRegistoDiarioDto registo)
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/RegistoDiario", registo);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<RegistoDiarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar registo: {ex.Message}");
                await HandleUnauthorizedAsync();
                return null;
            }
        }

        public async Task<bool> DeletarRegistoAsync(Guid id)
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                var response = await _httpClient.DeleteAsync($"api/RegistoDiario/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<EstatisticasHumorDto?> GetMyEstatisticasAsync(int dias = 30)
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                return await _httpClient.GetFromJsonAsync<EstatisticasHumorDto>($"api/RegistoDiario/me/estatisticas?dias={dias}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao buscar estatísticas: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                }
                return null;
            }
        }
        private async Task HandleUnauthorizedAsync()
        {
            SecureStorage.Remove("auth_token");
            await Shell.Current.DisplayAlert("Sessão Expirada", "Por favor, faça login novamente", "OK");
            await Shell.Current.GoToAsync("///LoginPage");
        }


        // ATUALIZAR: Desafios Semanais
        public async Task<ProgressoSemanalDto?> GetMyDesafiosSemanaisAsync()
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                return await _httpClient.GetFromJsonAsync<ProgressoSemanalDto>("api/Desafio/me/semanal");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                }
                return null;
            }
        }

        // ATUALIZAR: Concluir Desafio (já envia token)
        public async Task<ConcluirDesafioResponseDto?> ConcluirDesafioAsync(Guid desafioAtribuidoId)
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                var response = await _httpClient.PostAsync($"api/Desafio/{desafioAtribuidoId}/concluir", null);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                    return null;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ConcluirDesafioResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }

        // ATUALIZAR: Badges
        public async Task<ColecaoBadgesDto?> GetMyBadgesAsync()
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                return await _httpClient.GetFromJsonAsync<ColecaoBadgesDto>("api/Badge/me");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                }
                return null;
            }
        }

        public async Task<DashboardResponse?> GetDashboardDataAsync(Guid pacienteId)
        {
            // Este método JÁ sabe como adicionar o token de autenticação
            await ConfigureAuthHeaderAsync();

            try
            {
                
                var response = await _httpClient.GetFromJsonAsync<DashboardResponse>($"api/dashboard/{pacienteId}");
                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao buscar dashboard: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado na dashboard: {ex.Message}");
                return null;
            }
        }

        public async Task<EvolucaoResponseDto?> GetEvolucaoAsync(Guid pacienteId, int dias = 7)
        {
            await ConfigureAuthHeaderAsync();

            try
            {
                var response = await _httpClient.GetFromJsonAsync<EvolucaoResponseDto>(
                    $"api/evolucao/{pacienteId}?dias={dias}");

                Console.WriteLine($"Evolução carregada: {response?.TotalRegistos ?? 0} registos");
                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao buscar evolução: {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado na evolução: {ex.Message}");
                return null;
            }
        }

    }


}