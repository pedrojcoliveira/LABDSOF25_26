using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Entities;
using System.Text.Json;
using System.Net.Http.Json;

namespace MindCarePlus.API.Services
{
    public interface IRelatorioIAService
    {
        Task<RelatorioIA> GerarRelatorioSemanalAsync(Guid pacienteId);
    }

    public class RelatorioIAService : IRelatorioIAService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;

        public RelatorioIAService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["Groq:ApiKey"] ?? throw new Exception("Groq API Key não configurada");
            _model = configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";
            _baseUrl = configuration["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        }

        public async Task<RelatorioIA> GerarRelatorioSemanalAsync(Guid pacienteId)
        {
            // 1. Buscar dados do paciente
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(pacienteId);
            if (paciente == null) throw new Exception("Paciente não encontrado");

            // 2. Buscar registos dos últimos 7 dias
            var dataInicio = DateTime.UtcNow.AddDays(-7);
            var registos = await _unitOfWork.RegistosDiarios.FindAsync(r =>
                r.PacienteId == pacienteId && r.DataRegisto >= dataInicio);

            if (!registos.Any())
                throw new Exception("Sem dados suficientes para gerar relatório");

            // 3. Buscar desafios completados (se existirem)
            var desafios = new List<DesafioAtribuido>();
            try
            {
                desafios = (await _unitOfWork.DesafiosAtribuidos.FindAsync(d =>
                    d.UtilizadorId == pacienteId &&
                    d.DataConclusao.HasValue &&
                    d.DataConclusao.Value >= dataInicio)).ToList();
            }
            catch
            {
                // Se não houver desafios, continua
            }

            // 4. Calcular médias (SEM conversão, já está em 0-5)
            var mediaHumor = Math.Round(registos.Average(r => (decimal)r.NivelHumor), 1);
            var mediaEnergia = Math.Round(registos.Average(r => (decimal)r.NivelEnergia), 1);
            var mediaStress = Math.Round(registos.Average(r => (decimal)r.NivelStress), 1);

            // 5. Preparar dados para IA
            var dadosParaIA = new
            {
                paciente = paciente.Nome,
                periodo = $"{dataInicio:dd/MM/yyyy} - {DateTime.UtcNow:dd/MM/yyyy}",
                totalRegistos = registos.Count(),
                escalaNota = "Os níveis são apresentados numa escala de 0-5",
                mediaHumor = mediaHumor,
                mediaEnergia = mediaEnergia,
                mediaStress = mediaStress,
                registosDiarios = registos.OrderBy(r => r.DataRegisto).Select(r => new
                {
                    data = r.DataRegisto.ToString("dd/MM"),
                    humor = r.NivelHumor,     
                    energia = r.NivelEnergia,   
                    stress = r.NivelStress,     
                    notas = r.Notas
                }).ToList(),
                desafiosConcluidos = desafios.Count
            };

            var prompt = $@"
Você é um assistente clínico de saúde mental. Analise os seguintes dados de um paciente e gere um relatório semanal profissional para um psicólogo.

DADOS DO PACIENTE:
{JsonSerializer.Serialize(dadosParaIA, new JsonSerializerOptions { WriteIndented = true })}

INSTRUÇÕES:
1. Os níveis estão numa escala de 0-5:
   - Humor: 0 (muito baixo) a 5 (muito alto)
   - Energia: 0 (sem energia) a 5 (muita energia)
   - Stress: 0 (sem stress) a 5 (stress elevado)

2. Analise as tendências de cada métrica ao longo da semana
3. Identifique padrões preocupantes:
   - Humor <2.5 (baixo)
   - Energia <2.5 (baixa)
   - Stress >3.0 (elevado)
4. Avalie a evolução: melhorou, piorou ou manteve-se estável
5. Forneça recomendações clínicas práticas e específicas
6. Use linguagem profissional mas clara
7. Considere as notas do paciente se disponíveis

IMPORTANTE: Todos os campos devem ser strings de texto corrido, NÃO arrays ou listas.

FORMATO DA RESPOSTA (JSON):
{{
  ""tendenciasHumor"": ""Texto corrido com análise detalhada do humor em 2-3 frases"",
  ""tendenciasEnergia"": ""Texto corrido com análise detalhada da energia em 2-3 frases"",
  ""tendenciasStress"": ""Texto corrido com análise detalhada do stress em 2-3 frases"",
  ""recomendacoes"": ""Texto corrido com 4-5 recomendações clínicas específicas separadas por ponto e vírgula"",
  ""relatorioCompleto"": ""Texto corrido com relatório executivo completo em 3-4 parágrafos incluindo análise geral, padrões identificados e sugestões terapêuticas""
}}

ATENÇÃO: Responda APENAS com o JSON válido. Todos os valores devem ser strings de texto, NÃO use arrays [] ou listas. Sem markdown, sem ```json, apenas o objeto JSON puro com strings.
";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = JsonContent.Create(requestBody)
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API Groq: {response.StatusCode} - {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
            var respostaIA = responseData
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            // Limpar possíveis markdown tags
            respostaIA = respostaIA.Replace("```json", "").Replace("```", "").Trim();

            // 7. Parse resposta
            JsonElement relatorioJson;
            try
            {
                relatorioJson = JsonSerializer.Deserialize<JsonElement>(respostaIA);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Resposta da IA não está em formato JSON válido: {respostaIA}", ex);
            }

            // 8. Criar entidade
            var relatorio = new RelatorioIA
            {
                Id = Guid.NewGuid(),
                PacienteId = pacienteId,
                DataGeracao = DateTime.UtcNow,
                PeriodoInicio = dataInicio,
                PeriodoFim = DateTime.UtcNow,
                MediaHumor = mediaHumor,
                MediaEnergia = mediaEnergia,
                MediaStress = mediaStress,
                TendenciasHumor = relatorioJson.GetProperty("tendenciasHumor").GetString() ?? "",
                TendenciasEnergia = relatorioJson.GetProperty("tendenciasEnergia").GetString() ?? "",
                TendenciasStress = relatorioJson.GetProperty("tendenciasStress").GetString() ?? "",
                Recomendacoes = relatorioJson.GetProperty("recomendacoes").GetString() ?? "",
                RelatorioCompleto = relatorioJson.GetProperty("relatorioCompleto").GetString() ?? "",
                Lido = false
            };

            // 9. Guardar na BD
            await _unitOfWork.RelatoriosIA.AddAsync(relatorio);
            await _unitOfWork.SaveChangesAsync();

            return relatorio;
        }
    }
}