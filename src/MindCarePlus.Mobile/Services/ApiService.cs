using MindCarePlus.Shared.DTOs;
using MindCarePlus.Shared.DTOs.Paciente;
using MindCarePlus.Shared.DTOs.Psicologo;
using MindCarePlus.Shared.DTOs.RegistoDiario;
using MindCarePlus.Shared.DTOs.TarefaTerapeutica;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MindCarePlus.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;


    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    /// <summary>
    /// Lista todos os pacientes do psicólogo autenticado
    /// </summary>
    public async Task<List<PacienteResumoDto>> ListarMeusPacientesAsync()
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var pacientes = await _httpClient.GetFromJsonAsync<List<PacienteResumoDto>>("api/Paciente");
            return pacientes ?? new List<PacienteResumoDto>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erro ao listar pacientes: {ex.Message}");

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await Shell.Current.DisplayAlert("Acesso Negado",
                    "Apenas psicólogos podem aceder a esta funcionalidade", "OK");
            }

            return new List<PacienteResumoDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado ao listar pacientes: {ex.Message}");
            return new List<PacienteResumoDto>();
        }
    }


    /// <summary>
    /// Obtém os detalhes completos de um paciente específico
    /// User Story: UM.PS.04
    /// </summary>
    /// <param name="pacienteId">ID do paciente</param>
    public async Task<DetalhePacienteDto?> ObterDetalhesPacienteAsync(Guid pacienteId)
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var detalhes = await _httpClient.GetFromJsonAsync<DetalhePacienteDto>($"api/Paciente/{pacienteId}");
            return detalhes;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erro ao obter detalhes do paciente: {ex.Message}");

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await Shell.Current.DisplayAlert("Paciente Não Encontrado",
                    "Este paciente não existe ou não tem permissão de acesso", "OK");
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await Shell.Current.DisplayAlert("Acesso Negado",
                    "Não tem permissão para visualizar este paciente", "OK");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado ao obter detalhes do paciente: {ex.Message}");
            await Shell.Current.DisplayAlert("Erro",
                "Ocorreu um erro ao carregar os detalhes do paciente", "OK");
            return null;
        }
    }

    // ==================== RELATÓRIOS IA ====================

    public async Task<List<RelatorioIAResumoDto>> GetRelatoriosPacienteAsync(Guid pacienteId)
    {
        var response = await _httpClient.GetAsync($"api/RelatorioIA/paciente/{pacienteId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RelatorioIAResumoDto>>() ?? new();
    }

    public async Task<RelatorioIAResumoDto?> GerarRelatorioIAAsync(Guid pacienteId)
    {
        var response = await _httpClient.PostAsync($"api/RelatorioIA/gerar/{pacienteId}", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RelatorioIAResumoDto>();
    }

    public async Task<RelatorioIADetalheDto?> GetRelatorioDetalheAsync(Guid relatorioId)
    {
        var response = await _httpClient.GetAsync($"api/RelatorioIA/{relatorioId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RelatorioIADetalheDto>();
    }

    public async Task MarcarRelatorioLidoAsync(Guid relatorioId)
    {
        var response = await _httpClient.PatchAsync($"api/RelatorioIA/{relatorioId}/marcar-lido", null);
        response.EnsureSuccessStatusCode();
    }



    /// <summary>
    /// Cria uma tarefa terapêutica para um paciente
    /// User Story: UM.PS.05
    /// </summary>
    public async Task<TarefaTerapeuticaDto?> CriarTarefaTerapeuticaAsync(CriarTarefaTerapeuticaDto dto)
    {
        await ConfigureAuthHeaderAsync(); // Garante que o token está lá

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/TarefaTerapeutica", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync(); // Lida com token expirado
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TarefaTerapeuticaDto>();
            }

            // Log de erro se não for sucesso
            Console.WriteLine($"Erro API: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar tarefa: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Lista tarefas terapêuticas do paciente
    /// </summary>
    public async Task<List<TarefaTerapeuticaDto>> ListarTarefasPacienteAsync(Guid pacienteId)
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var response = await _httpClient.GetAsync($"api/TarefaTerapeutica/paciente/{pacienteId}");

            if (response.IsSuccessStatusCode)
            {
                var tarefas = await response.Content.ReadFromJsonAsync<List<TarefaTerapeuticaDto>>();
                return tarefas ?? new List<TarefaTerapeuticaDto>();
            }

            return new List<TarefaTerapeuticaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao listar tarefas: {ex.Message}");
            return new List<TarefaTerapeuticaDto>();
        }
    }


    /// <summary>
    /// Lista MINHAS tarefas terapêuticas (paciente autenticado)
    /// </summary>
    public async Task<List<TarefaTerapeuticaDto>> ListarMinhasTarefasTerapeuticasAsync()
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var tarefas = await _httpClient.GetFromJsonAsync<List<TarefaTerapeuticaDto>>("api/TarefaTerapeutica/me");
            return tarefas ?? new List<TarefaTerapeuticaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao listar minhas tarefas: {ex.Message}");
            return new List<TarefaTerapeuticaDto>();
        }
    }


    /// <summary>
    /// GET: Dashboard do Psicólogo
    /// UM.PS.06
    /// </summary>
    public async Task<DashboardPsicologoDto?> GetDashboardPsicologoAsync()
    {
        try
        {
            // 1. Verificar autenticação e token
            if (!await IsAuthenticatedAsync())
            {
                Console.WriteLine("❌ Não autenticado");
                await HandleUnauthorizedAsync();
                return null;
            }

            // 2. Configurar headers com token
            await ConfigureAuthHeaderAsync();

            Console.WriteLine("🔍 A carregar dashboard do psicólogo...");
            Console.WriteLine($"📍 URL: api/DashboardPsicologo");
            Console.WriteLine($"🔐 Authorization: Bearer [token]");

            // 3. Fazer request
            var response = await _httpClient.GetAsync("api/DashboardPsicologo");

            // 4. Log do status
            Console.WriteLine($"📊 Status Code: {response.StatusCode}");

            // 5. Tratamento de erros HTTP
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("❌ Token expirado ou inválido");
                await HandleUnauthorizedAsync();
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("❌ Acesso negado - role não permitido");
                await Shell.Current.DisplayAlert(
                    "Acesso Negado",
                    "Apenas psicólogos podem aceder a este dashboard",
                    "OK");
                await HandleUnauthorizedAsync();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erro HTTP {response.StatusCode}");
                Console.WriteLine($"   Resposta: {errorContent}");

                await Shell.Current.DisplayAlert(
                    "Erro ao carregar",
                    $"Erro {response.StatusCode}: {errorContent}",
                    "OK");
                return null;
            }

            // 6. Desserializar resposta
            var dashboard = await response.Content.ReadFromJsonAsync<DashboardPsicologoDto>();

            if (dashboard == null)
            {
                Console.WriteLine("❌ Dashboard nulo após desserialização");
                return null;
            }

            Console.WriteLine("✅ Dashboard carregado com sucesso");
            Console.WriteLine($"   - Pacientes Ativos: {dashboard.TotalPacientesAtivos}");
            Console.WriteLine($"   - Sessões Este Mês: {dashboard.SessoesEsteMes}");
            Console.WriteLine($"   - Mensagens Não Lidas: {dashboard.MensagensNaoLidas}");
            Console.WriteLine($"   - Alertas: {dashboard.Alertas?.Count ?? 0}");
            Console.WriteLine($"   - Pacientes Recentes: {dashboard.PacientesRecentes?.Count ?? 0}");

            return dashboard;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ HttpRequestException: {ex.Message}");
            Console.WriteLine($"   StatusCode: {ex.StatusCode}");
            Console.WriteLine($"   Inner: {ex.InnerException?.Message}");

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
            }

            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"❌ Erro ao desserializar JSON: {ex.Message}");
            Console.WriteLine($"   Path: {ex.Path}");
            Console.WriteLine($"   LineNumber: {ex.LineNumber}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"❌ Timeout na requisição: {ex.Message}");
            await Shell.Current.DisplayAlert(
                "Timeout",
                "A requisição demorou muito tempo. Tente novamente.",
                "OK");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro inesperado: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Verifica se o utilizador está autenticado
    /// </summary>
    private async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            var isAuthenticated = !string.IsNullOrEmpty(token);
            Console.WriteLine($"🔐 Autenticado: {isAuthenticated}");
            return isAuthenticated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao verificar autenticação: {ex.Message}");
            return false;
        }
    }
}


