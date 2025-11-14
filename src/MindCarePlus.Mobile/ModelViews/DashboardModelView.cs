/* using CommunityToolkit.Mvvm.ComponentModel;
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
            System.Diagnostics.Debug.WriteLine("Comando executado!");
            try
            {
                await Shell.Current.GoToAsync("evolucao"); 
                System.Diagnostics.Debug.WriteLine("Navegação OK!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
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

*/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using Microcharts;
using SkiaSharp;

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

        [ObservableProperty]
        private Chart graficoHumorDashboard;

        private Guid _pacienteId;

        public async Task LoadDashboardAsync(Guid pacienteId)
        {
            _pacienteId = pacienteId;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<DashboardResponse>($"api/dashboard/{pacienteId}");
                if (response == null) throw new Exception("Resposta vazia");

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

                // Criar gráfico de linha do humor
                var humorEntries = response.EvolucaoHumor.Select(e => new ChartEntry(e.NivelHumor * 10)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelHumor.ToString(),
                    Color = SKColor.Parse("#6C63FF")
                }).ToList();

                GraficoHumorDashboard = new LineChart
                {
                    Entries = humorEntries,
                    LineMode = LineMode.Spline,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 15,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 28,
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0,
                    MaxValue = 100
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dashboard: {ex.Message}");
                CarregarDadosMock();
            }
        }

        private void CarregarDadosMock()
        {
            NomeUtilizador = "Paciente de Teste";
            NivelAtual = 1;
            PontosAtuais = 100;
            PontosParaProximoNivel = 600;
            ProgressoPercentagem = 0.17;
            SequenciaDias = 2;
            TotalRegistos = 7;
            TotalBadges = 0;

            // Mock do gráfico de humor
            var humorMockEntries = new List<ChartEntry>
            {
                new ChartEntry(60) { Label = "Seg", ValueLabel = "6", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(70) { Label = "Ter", ValueLabel = "7", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(50) { Label = "Qua", ValueLabel = "5", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(80) { Label = "Qui", ValueLabel = "8", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(70) { Label = "Sex", ValueLabel = "7", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(80) { Label = "Sáb", ValueLabel = "8", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(90) { Label = "Dom", ValueLabel = "9", Color = SKColor.Parse("#6C63FF") }
            };

            GraficoHumorDashboard = new LineChart
            {
                Entries = humorMockEntries,
                LineMode = LineMode.Spline,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 15,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 28,
                ValueLabelOrientation = Orientation.Horizontal,
                MinValue = 0,
                MaxValue = 100
            };
        }

        [RelayCommand]
        private async Task RegistarHumor()
        {
            await Shell.Current.GoToAsync("registarhumor");
        }

        [RelayCommand]
        private async Task VerGraficoEvolucao()
        {
            await Shell.Current.GoToAsync($"evolucao?pacienteId={_pacienteId}");
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