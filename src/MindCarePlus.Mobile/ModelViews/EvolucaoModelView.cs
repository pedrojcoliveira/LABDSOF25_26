using CommunityToolkit.Mvvm.ComponentModel;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using Microcharts;
using SkiaSharp;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class EvolucaoModelView : ObservableObject
    {
        private readonly ApiService _apiService;  
        private readonly AuthService _authService;

        public EvolucaoModelView(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
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

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        [ObservableProperty]
        private bool temDados = true;

        public async Task LoadEvolucaoAsync(Guid? pacienteId = null, int dias = 7)
        {
            IsLoading = true;
            MensagemErro = string.Empty;
            TemDados = true;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine(" Carregando evolução...");

                // Se não foi passado pacienteId, obtém do token
                if (pacienteId == null)
                {
                    if (!await _authService.IsAuthenticatedAsync())
                    {
                        Console.WriteLine("Não autenticado!");
                        await Shell.Current.GoToAsync("//login");
                        return;
                    }

                    pacienteId = await _authService.GetCurrentUserIdAsync();
                    if (pacienteId == null)
                    {
                        Console.WriteLine("PacienteId é null!");
                        MensagemErro = "Erro ao obter dados do utilizador.";
                        await Shell.Current.GoToAsync("//login");
                        return;
                    }
                }

                Console.WriteLine($"PacienteId: {pacienteId}");
                Console.WriteLine($"Dias: {dias}");

                // Busca dados da API usando ApiService (com token!)
                var response = await _apiService.GetEvolucaoAsync(pacienteId.Value, dias);

                if (response == null)
                {
                    Console.WriteLine("Response é null!");
                    MensagemErro = "Erro ao carregar dados.";
                    TemDados = false;
                    return;
                }

                Console.WriteLine($"   Evolução recebida:");
                Console.WriteLine($"   Nome: {response.PacienteNome}");
                Console.WriteLine($"   Período: {response.PeriodoInicio:dd/MM/yyyy} - {response.PeriodoFim:dd/MM/yyyy}");
                Console.WriteLine($"   Total Registos: {response.TotalRegistos}");
                Console.WriteLine($"   Evolução: {response.Evolucao?.Count ?? 0} dias");

                // Preenche informações gerais
                PacienteNome = response.PacienteNome;
                Periodo = $"{response.PeriodoInicio:dd/MM/yyyy} - {response.PeriodoFim:dd/MM/yyyy}";
                TotalRegistos = response.TotalRegistos;

                // Verifica se há dados
                if (response.Evolucao == null || !response.Evolucao.Any())
                {
                    Console.WriteLine("Sem dados de evolução");
                    TemDados = false;
                    MensagemErro = "Ainda não há registos suficientes para mostrar a evolução.";
                    return;
                }

                TemDados = true;

                // Cria gráfico de HUMOR
                var humorEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelHumor)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelHumor.ToString("F1"),
                    Color = SKColor.Parse("#6C63FF")
                }).ToList();

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

                Console.WriteLine("Gráfico de Humor criado");

                // Cria gráfico de ENERGIA
                var energiaEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelEnergia)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelEnergia.ToString("F1"),
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
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0,
                    MaxValue = 10
                };

                Console.WriteLine("Gráfico de Energia criado");

                // Cria gráfico de STRESS
                var stressEntries = response.Evolucao.Select(e => new ChartEntry(e.NivelStress)
                {
                    Label = e.DiaSemana,
                    ValueLabel = e.NivelStress.ToString("F1"),
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
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0,
                    MaxValue = 10
                };

                Console.WriteLine("Gráfico de Stress criado");
                Console.WriteLine($"{response.Evolucao.Count} dias processados");
                Console.WriteLine("========================================");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Erro HTTP: {ex.Message}");
                MensagemErro = "Erro de conexão com o servidor.";
                TemDados = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                MensagemErro = $"Erro: {ex.Message}";
                TemDados = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}