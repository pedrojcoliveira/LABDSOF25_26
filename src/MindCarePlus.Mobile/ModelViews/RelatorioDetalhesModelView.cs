using CommunityToolkit.Mvvm.ComponentModel;
using MindCarePlus.Shared.DTOs;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class RelatorioDetalhesViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        public RelatorioDetalhesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [ObservableProperty]
        private string pacienteNome = string.Empty;

        [ObservableProperty]
        private string periodo = string.Empty;

        [ObservableProperty]
        private decimal mediaHumor;

        [ObservableProperty]
        private decimal mediaEnergia;

        [ObservableProperty]
        private decimal mediaStress;

        [ObservableProperty]
        private string tendenciasHumor = string.Empty;

        [ObservableProperty]
        private string tendenciasEnergia = string.Empty;

        [ObservableProperty]
        private string tendenciasStress = string.Empty;

        [ObservableProperty]
        private string recomendacoes = string.Empty;

        [ObservableProperty]
        private string relatorioCompleto = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        public async Task LoadRelatorioAsync(Guid relatorioId)
        {
            IsLoading = true;
            MensagemErro = string.Empty;

            try
            {
                // ← CORRIGIDO: "RelatorioIA" (com "i")
                var relatorio = await _httpClient.GetFromJsonAsync<RelatorioIADto>($"api/RelatorioIA/{relatorioId}");

                if (relatorio == null)
                {
                    MensagemErro = "Relatório não encontrado";
                    return;
                }

                PacienteNome = relatorio.PacienteNome;
                Periodo = $"{relatorio.PeriodoInicio} - {relatorio.PeriodoFim}";
                MediaHumor = relatorio.MediaHumor;
                MediaEnergia = relatorio.MediaEnergia;
                MediaStress = relatorio.MediaStress;
                TendenciasHumor = relatorio.TendenciasHumor;
                TendenciasEnergia = relatorio.TendenciasEnergia;
                TendenciasStress = relatorio.TendenciasStress;
                Recomendacoes = relatorio.Recomendacoes;
                RelatorioCompleto = relatorio.RelatorioCompleto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar detalhes: {ex.Message}");
                MensagemErro = $"Erro ao carregar relatório: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}