using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using MindCarePlus.Mobile.Messages;
using Microcharts;
using SkiaSharp;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;  // ← USA ApiService!
        private readonly AuthService _authService;

        public DashboardViewModel(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;

            BadgesConquistadas = new ObservableCollection<BadgeConquistadaDto>();
            EvolucaoHumor = new ObservableCollection<DiaHumorDto>();

            WeakReferenceMessenger.Default.Register<RegistoCriadoMessage>(this, async (r, m) =>
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔔 Mensagem recebida: Registo criado!");
                Console.WriteLine("🔄 Atualizando dashboard...");
                Console.WriteLine("========================================");

                await LoadDashboardAsync();
            });
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
        private ObservableCollection<BadgeConquistadaDto> badgesConquistadas;

        [ObservableProperty]
        private ObservableCollection<DiaHumorDto> evolucaoHumor;

        [ObservableProperty]
        private Chart graficoHumorDashboard;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        [ObservableProperty]
        private bool temDados = true;

        private Guid _pacienteId;

        public async Task LoadDashboardAsync()
        {
            IsLoading = true;
            MensagemErro = string.Empty;
            TemDados = true;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔍 Iniciando carregamento da Dashboard");

                // 1. Verifica autenticação
                if (!await _authService.IsAuthenticatedAsync())
                {
                    Console.WriteLine("❌ Não autenticado!");
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // 2. Verifica se é Paciente
                var role = await _authService.GetUserRoleAsync();
                Console.WriteLine($"🔍 Role: {role}");

                if (role != "Paciente")
                {
                    MensagemErro = "Acesso negado. Apenas pacientes podem aceder à dashboard.";
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // 3. Obtém ID do paciente do token
                var pacienteId = await _authService.GetCurrentUserIdAsync();
                if (pacienteId == null)
                {
                    Console.WriteLine("❌ PacienteId é null!");
                    MensagemErro = "Erro ao obter dados do utilizador.";
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                _pacienteId = pacienteId.Value;
                Console.WriteLine($"✅ PacienteId: {_pacienteId}");

                // 4. Busca dados da dashboard usando ApiService (com token!)
                var response = await _apiService.GetDashboardDataAsync(_pacienteId);

                if (response == null)
                {
                    Console.WriteLine("❌ Response é null!");
                    MensagemErro = "Erro ao carregar dados.";
                    TemDados = false;
                    return;
                }

                Console.WriteLine($"✅ Dashboard recebida:");
                Console.WriteLine($"   Nome: {response.NomeUtilizador}");
                Console.WriteLine($"   Nível: {response.NivelAtual}");
                Console.WriteLine($"   Pontos: {response.PontosAtuais}/{response.PontosParaProximoNivel}");
                Console.WriteLine($"   Sequência: {response.SequenciaDias} dias");
                Console.WriteLine($"   Registos: {response.TotalRegistos}");
                Console.WriteLine($"   Badges: {response.TotalBadges}");
                Console.WriteLine($"   Evolução: {response.EvolucaoHumor?.Count ?? 0} dias");

                // 5. Preenche dados
                NomeUtilizador = response.NomeUtilizador;
                NivelAtual = response.NivelAtual;
                PontosAtuais = response.PontosAtuais;
                PontosParaProximoNivel = response.PontosParaProximoNivel;
                ProgressoPercentagem = response.ProgressoPercentagem;
                SequenciaDias = response.SequenciaDias;
                TotalRegistos = response.TotalRegistos;
                TotalBadges = response.TotalBadges;

                // 6. Badges
                BadgesConquistadas.Clear();
                if (response.BadgesConquistadas != null && response.BadgesConquistadas.Any())
                {
                    foreach (var badge in response.BadgesConquistadas)
                    {
                        BadgesConquistadas.Add(badge);
                    }
                    Console.WriteLine($"✅ {BadgesConquistadas.Count} badges adicionadas");
                }

                // 7. Evolução do humor
                EvolucaoHumor.Clear();
                if (response.EvolucaoHumor != null && response.EvolucaoHumor.Any())
                {
                    foreach (var dia in response.EvolucaoHumor)
                    {
                        EvolucaoHumor.Add(dia);
                        Console.WriteLine($"   {dia.DiaSemana}: {dia.NivelHumor}");
                    }
                    Console.WriteLine($"✅ {EvolucaoHumor.Count} dias de evolução");

                    // 8. Gráfico
                    var humorEntries = response.EvolucaoHumor.Select(e => new ChartEntry(e.NivelHumor * 10)
                    {
                        Label = e.DiaSemana,
                        ValueLabel = e.NivelHumor.ToString("F1"),
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
                    TemDados = true;
                }
                else
                {
                    Console.WriteLine("⚠️ Sem dados de evolução");
                    GraficoHumorDashboard = null;
                    TemDados = false;
                }

                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                MensagemErro = $"Erro: {ex.Message}";
                TemDados = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RegistarHumor()
        {
            await Shell.Current.GoToAsync("RegistoHumor");
        }

        [RelayCommand]
        private async Task VerGraficoEvolucao()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine($"🔍 Comando VerGraficoEvolucao executado!");
                Console.WriteLine($"🔍 PacienteId: {_pacienteId}");
                Console.WriteLine($"🔍 Navegando para: evolucao?pacienteId={_pacienteId}");
                Console.WriteLine("========================================");

                await Shell.Current.GoToAsync($"evolucao?pacienteId={_pacienteId}");

                Console.WriteLine("✅ Navegação concluída!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao navegar: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    $"Erro ao navegar: {ex.Message}",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task VerDesafios()
        {
            await Shell.Current.GoToAsync("DesafiosPage");
        }

        [RelayCommand]
        private async Task ContactarPsicologo()
        {
            await Shell.Current.GoToAsync("contactar");
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }

        [RelayCommand]
        private async Task ShowBadgeDetails(BadgeConquistadaDto badge)
        {
            if (badge == null) return;

            await Shell.Current.DisplayAlert(
                $"{badge.ImagemUrl} {badge.Nome}",
                $"{badge.Descricao}\n\nNível: {badge.Nivel}\n✨ Conquistada!",
                "Fechar"
            );
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadDashboardAsync();
        }
    }
}