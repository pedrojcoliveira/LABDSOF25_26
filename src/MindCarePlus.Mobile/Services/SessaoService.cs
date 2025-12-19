using System.Net.Http.Json;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.Mobile.Services
{
    public class SessaoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public SessaoService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<SessaoDto?> SolicitarSessaoAsync(SolicitarSessaoDto request)
        {
            var token = await _authService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJsonAsync("api/sessoes/solicitar", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SessaoDto>();
        }

        public async Task<List<SessaoDto>> GetMinhasSolicitacoesAsync()
        {
            var token = await _authService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/sessoes/minhas-solicitacoes");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<SessaoDto>>() ?? new List<SessaoDto>();
        }

        public async Task<SessaoDto?> GetSessaoAsync(Guid id)
        {
            var token = await _authService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/sessoes/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SessaoDto>();
        }
        public async Task<bool> CancelarSessaoAsync(Guid sessaoId)
        {
            var token = await _authService.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PutAsync($"api/sessoes/{sessaoId}/cancelar", null);

            return response.IsSuccessStatusCode;
        }

    }
}