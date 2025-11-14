using System.Net.Http.Json;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        
  
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
        }

        // GET: Buscar registos por paciente
        public async Task<List<RegistoDiarioDto>> GetRegistosPorPacienteAsync(Guid pacienteId)
        {
            try
            {
                var registos = await _httpClient.GetFromJsonAsync<List<RegistoDiarioDto>>($"RegistoDiario/paciente/{pacienteId}");
                return registos ?? new List<RegistoDiarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar registos: {ex.Message}");
                return new List<RegistoDiarioDto>();
            }
        }

        // POST: Criar novo registo
        public async Task<RegistoDiarioDto?> CriarRegistoAsync(CriarRegistoDiarioDto registo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("RegistoDiario", registo);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<RegistoDiarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar registo: {ex.Message}");
                return null;
            }
        }

        // DELETE: Apagar registo
        public async Task<bool> DeletarRegistoAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"RegistoDiario/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // GET: Estatísticas
        public async Task<EstatisticasHumorDto?> GetEstatisticasAsync(Guid pacienteId, int dias = 30)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<EstatisticasHumorDto>($"RegistoDiario/paciente/{pacienteId}/estatisticas?dias={dias}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar estatísticas: {ex.Message}");
                return null;
            }
        }

        public async Task<ProgressoSemanalDto?> GetDesafiosSemanaisAsync(Guid pacienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProgressoSemanalDto>($"Desafio/paciente/{pacienteId}/semanal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }

        public async Task<ConcluirDesafioResponseDto?> ConcluirDesafioAsync(Guid desafioAtribuidoId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"Desafio/{desafioAtribuidoId}/concluir", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ConcluirDesafioResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }

        // ===== BADGES =====
        public async Task<ColecaoBadgesDto?> GetBadgesAsync(Guid pacienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ColecaoBadgesDto>($"Badge/paciente/{pacienteId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }
    }
}