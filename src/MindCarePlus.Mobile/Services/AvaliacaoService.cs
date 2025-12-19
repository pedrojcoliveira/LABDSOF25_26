using MindCarePlus.Shared.DTOs;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.Services
{
    public class AvaliacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public AvaliacaoService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<AvaliacaoDTOs?> CriarAvaliacaoAsync(CriarAvaliacaoDto avaliacaoDto)
        {
            try
            {
                var token = await _authService.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/avaliacoes", avaliacaoDto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AvaliacaoDTOs>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar avaliação: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exceção ao criar avaliação: {ex.Message}");
                return null;
            }
        }

        public async Task<AvaliacaoDTOs?> GetAvaliacaoPorSessaoAsync(Guid sessaoId)
        {
            try
            {
                var token = await _authService.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"api/avaliacoes/sessao/{sessaoId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AvaliacaoDTOs>();
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exceção ao obter avaliação: {ex.Message}");
                return null;
            }
        }
    }
}