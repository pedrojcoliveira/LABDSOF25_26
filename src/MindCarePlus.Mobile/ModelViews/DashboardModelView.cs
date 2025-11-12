using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        public DashboardViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [ObservableProperty]
        private string nomeUtilizador = string.Empty;

        [ObservableProperty]
        private int nivelAtual;

        [ObservableProperty]
        private int pontosAtuais;

        [ObservableProperty]
        private int pontosParaProximoNivel;

        [ObservableProperty]
        private double progressoPercentagem;

        [ObservableProperty]
        private int sequenciaDias;

        [ObservableProperty]
        private int totalRegistos;

        [ObservableProperty]
        private int totalBadges;

        [ObservableProperty]
        private ObservableCollection<BadgeConquistadaDto> badgesConquistadas = new();

        [ObservableProperty]
        private ObservableCollection<DiaHumorDto> evolucaoHumor = new();

        private Guid _pacienteId;

        public async Task LoadDashboardAsync(Guid pacienteId)
        {
            _pacienteId = pacienteId;

            var response = await _httpClient.GetFromJsonAsync<DashboardResponse>($"api/dashboard/{pacienteId}");
            if (response == null) return;

            NomeUtilizador = response.NomeUtilizador;
            NivelAtual = response.NivelAtual;
            PontosAtuais = response.PontosAtuais;
            PontosParaProximoNivel = response.PontosParaProximoNivel;
            ProgressoPercentagem = response.ProgressoPercentagem;
            SequenciaDias = response.SequenciaDias;
            TotalRegistos = response.TotalRegistos;
            TotalBadges = response.TotalBadges;

            BadgesConquistadas.Clear();
            foreach (var badge in response.BadgesConquistadas)
            {
                BadgesConquistadas.Add(badge);
            }

            EvolucaoHumor.Clear();
            foreach (var dia in response.EvolucaoHumor)
            {
                EvolucaoHumor.Add(dia);
            }
        }

        [RelayCommand]
        private async Task RegistarHumor()
        {
            await Shell.Current.GoToAsync("registarhumor");
        }

        [RelayCommand]
        private async Task VerGraficoEvolucao()
        {
            await Shell.Current.GoToAsync("evolucao");
        }

        [RelayCommand]
        private async Task VerDesafios()
        {
            await Shell.Current.GoToAsync("desafios");
        }

        [RelayCommand]
        private async Task ContactarPsicologo()
        {
            await Shell.Current.GoToAsync("contactar");
        }
    }
}