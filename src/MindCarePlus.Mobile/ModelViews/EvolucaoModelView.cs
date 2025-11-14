/* using CommunityToolkit.Mvvm.ComponentModel;
using MindCarePlus.Shared.DTOs;
using System.Net.Http.Json;
using Microcharts;
using SkiaSharp;


namespace MindCarePlus.Mobile.ModelViews
{
    public partial class EvolucaoModelView : ObservableObject
    {
        private readonly HttpClient _httpClient;

        public EvolucaoModelView(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [ObservableProperty]
        private string pacienteNome = string.Empty;

        [ObservableProperty]
        private string periodo = string.Empty;

        [ObservableProperty]
        private int totalRegistos;

        [ObservableProperty]
        private Chart graficoHumor;

        [ObservableProperty]
        private Chart graficoEnergia;

        [ObservableProperty]
        private Chart graficoStress;

        public async Task LoadEvolucaoAsync(Guid pacienteId, int dias = 7)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<EvolucaoResponseDto>($"api/evolucao/{pacienteId}?dias={dias}");
                if (response == null) return;

                PacienteNome = response.PacienteNome;
                Periodo = $"{response.PeriodoInicio} - {response.PeriodoFim}";
                TotalRegistos = response.TotalRegistos;

                // Criar gráficos
                var entries = response.Evolucao.Select(e => new ChartEntry(e.NivelHumor)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelHumor.ToString(),
                    Color = SKColor.Parse("#6C63FF")
                }).ToList();

                GraficoHumor = new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0,
                    MaxValue = 10
                };

                var energiaEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelEnergia)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelEnergia.ToString(),
                    Color = SKColor.Parse("#4CAF50")
                }).ToList();

                GraficoEnergia = new LineChart
                {
                    Entries = energiaEntries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    MinValue = 0,
                    MaxValue = 10
                };

                var stressEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelStress)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelStress.ToString(),
                    Color = SKColor.Parse("#FF5252")
                }).ToList();

                GraficoStress = new LineChart
                {
                    Entries = stressEntries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    MinValue = 0,
                    MaxValue = 10
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar evolução: {ex.Message}");
            }
        }
    }
}

*/

using CommunityToolkit.Mvvm.ComponentModel;
using MindCarePlus.Shared.DTOs;
using System.Net.Http.Json;
using Microcharts;
using SkiaSharp;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class EvolucaoModelView : ObservableObject
    {
        private readonly HttpClient _httpClient;

        public EvolucaoModelView(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [ObservableProperty]
        private string pacienteNome = string.Empty;

        [ObservableProperty]
        private string periodo = string.Empty;

        [ObservableProperty]
        private int totalRegistos;

        [ObservableProperty]
        private Chart graficoHumor;

        [ObservableProperty]
        private Chart graficoEnergia;

        [ObservableProperty]
        private Chart graficoStress;

        public async Task LoadEvolucaoAsync(Guid pacienteId, int dias = 7)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<EvolucaoResponseDto>($"api/evolucao/{pacienteId}?dias={dias}");
                if (response == null) throw new Exception("Resposta vazia da API");

                PacienteNome = response.PacienteNome;
                Periodo = $"{response.PeriodoInicio} - {response.PeriodoFim}";
                TotalRegistos = response.TotalRegistos;

                // Criar gráficos
                var entries = response.Evolucao.Select(e => new ChartEntry(e.NivelHumor)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelHumor.ToString(),
                    Color = SKColor.Parse("#6C63FF")
                }).ToList();

                GraficoHumor = new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0,
                    MaxValue = 10
                };

                var energiaEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelEnergia)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelEnergia.ToString(),
                    Color = SKColor.Parse("#4CAF50")
                }).ToList();

                GraficoEnergia = new LineChart
                {
                    Entries = energiaEntries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    MinValue = 0,
                    MaxValue = 10
                };

                var stressEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelStress)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelStress.ToString(),
                    Color = SKColor.Parse("#FF5252")
                }).ToList();

                GraficoStress = new LineChart
                {
                    Entries = stressEntries,
                    LineMode = LineMode.Straight,
                    LineSize = 4,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    MinValue = 0,
                    MaxValue = 10
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar evolução: {ex.Message}");
                CarregarDadosMock();
            }
        }

        private void CarregarDadosMock()
        {
            PacienteNome = "João Silva";
            Periodo = "05/01/2025 - 11/01/2025";
            TotalRegistos = 7;

            // Dados mock - Gráfico de Humor
            var humorEntries = new List<ChartEntry>
            {
                new ChartEntry(6) { Label = "Seg", ValueLabel = "6", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(7) { Label = "Ter", ValueLabel = "7", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(5) { Label = "Qua", ValueLabel = "5", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(8) { Label = "Qui", ValueLabel = "8", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(7) { Label = "Sex", ValueLabel = "7", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(8) { Label = "Sáb", ValueLabel = "8", Color = SKColor.Parse("#6C63FF") },
                new ChartEntry(9) { Label = "Dom", ValueLabel = "9", Color = SKColor.Parse("#6C63FF") }
            };

            GraficoHumor = new LineChart
            {
                Entries = humorEntries,
                LineMode = LineMode.Straight,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 18,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 32,
                ValueLabelOrientation = Orientation.Horizontal,
                MinValue = 0,
                MaxValue = 10
            };

            // Dados mock - Gráfico de Energia
            var energiaEntries = new List<ChartEntry>
            {
                new ChartEntry(7) { Label = "Seg", ValueLabel = "7", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(8) { Label = "Ter", ValueLabel = "8", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(6) { Label = "Qua", ValueLabel = "6", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(9) { Label = "Qui", ValueLabel = "9", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(8) { Label = "Sex", ValueLabel = "8", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(9) { Label = "Sáb", ValueLabel = "9", Color = SKColor.Parse("#4CAF50") },
                new ChartEntry(9) { Label = "Dom", ValueLabel = "9", Color = SKColor.Parse("#4CAF50") }
            };

            GraficoEnergia = new LineChart
            {
                Entries = energiaEntries,
                LineMode = LineMode.Straight,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 18,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 32,
                MinValue = 0,
                MaxValue = 10
            };

            // Dados mock - Gráfico de Stress
            var stressEntries = new List<ChartEntry>
            {
                new ChartEntry(4) { Label = "Seg", ValueLabel = "4", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(3) { Label = "Ter", ValueLabel = "3", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(5) { Label = "Qua", ValueLabel = "5", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(2) { Label = "Qui", ValueLabel = "2", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(3) { Label = "Sex", ValueLabel = "3", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(2) { Label = "Sáb", ValueLabel = "2", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(2) { Label = "Dom", ValueLabel = "2", Color = SKColor.Parse("#FF5252") }
            };

            GraficoStress = new LineChart
            {
                Entries = stressEntries,
                LineMode = LineMode.Straight,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 18,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 32,
                MinValue = 0,
                MaxValue = 10
            };
        }
    }
}