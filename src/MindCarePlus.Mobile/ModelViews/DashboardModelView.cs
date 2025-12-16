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
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private readonly ChatService _chatService;

        public DashboardViewModel(ApiService apiService, AuthService authService, ChatService chatService)
        {
            _apiService = apiService;
            _authService = authService;
            _chatService = chatService;

            BadgesConquistadas = new ObservableCollection<BadgeConquistadaDto>();
            EvolucaoHumor = new ObservableCollection<DiaHumorDto>();

            WeakReferenceMessenger.Default.Register<RegistoCriadoMessage>(this, async (r, m) =>
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔔 Mensagem recebida: Registo criado!");
                Console.WriteLine("🔄 A atualizar dashboard...");
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

        [ObservableProperty]
        private int totalMensagensNaoLidas;

        private Guid _pacienteId;

        #region Propriedades - Relatório IA

        [ObservableProperty]
        private bool isGerarRelatorio;

        [ObservableProperty]
        private bool mostrarRelatorio;

        [ObservableProperty]
        private string relatorioPeriodo = string.Empty;

        [ObservableProperty]
        private decimal relatorioMediaHumor;

        [ObservableProperty]
        private decimal relatorioMediaEnergia;

        [ObservableProperty]
        private decimal relatorioMediaStress;

        [ObservableProperty]
        private string relatorioTendenciasHumor = string.Empty;

        [ObservableProperty]
        private string relatorioTendenciasEnergia = string.Empty;

        [ObservableProperty]
        private string relatorioTendenciasStress = string.Empty;

        [ObservableProperty]
        private string relatorioRecomendacoes = string.Empty;

        [ObservableProperty]
        private string relatorioCompleto = string.Empty;

        #endregion

        public int PontosParaProximoNivel
        {
            get
            {
                // Cada nível precisa de 600 pontos
                int pontosNecessarios = NivelAtual * 600;
                int pontosFaltam = pontosNecessarios - PontosAtuais;
                return Math.Max(0, pontosFaltam);
            }
        }

        public double ProgressoPercentagem
        {
            get
            {
                int pontosNecessarios = NivelAtual * 600;
                if (pontosNecessarios == 0) return 0;

                double progresso = (double)PontosAtuais / pontosNecessarios;
                return Math.Min(1.0, progresso);
            }
        }

        partial void OnPontosAtuaisChanged(int value)
        {
            OnPropertyChanged(nameof(PontosParaProximoNivel));
            OnPropertyChanged(nameof(ProgressoPercentagem));
        }

        partial void OnNivelAtualChanged(int value)
        {
            OnPropertyChanged(nameof(PontosParaProximoNivel));
            OnPropertyChanged(nameof(ProgressoPercentagem));
        }

        public async Task LoadDashboardAsync()
        {
            IsLoading = true;
            MensagemErro = string.Empty;
            TemDados = true;

            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("🔍 A iniciar carregamento da Dashboard");

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

                // CARREGAR MENSAGENS NÃO LIDAS
                try
                {
                    TotalMensagensNaoLidas = await _chatService.ContarMensagensNaoLidasAsync();
                    Console.WriteLine($"📬 {TotalMensagensNaoLidas} mensagens não lidas");
                }
                catch (Exception chatEx)
                {
                    Console.WriteLine($"⚠️ Erro ao carregar mensagens: {chatEx.Message}");
                    TotalMensagensNaoLidas = 0;
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
                Console.WriteLine($"🔍 A navegar para: evolucao?pacienteId={_pacienteId}");
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
            try
            {
                await Shell.Current.GoToAsync("//ContactarPsicologoPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao navegar para ContactarPsicologoPage: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro",
                    $"Erro ao abrir página de contacto: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task AbrirMensagens()
        {
            try
            {
                await Shell.Current.GoToAsync("//ConversasPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao navegar para ConversasPage: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro",
                    $"Erro ao abrir mensagens: {ex.Message}", "OK");
            }
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

        [RelayCommand]
        public async Task GerarRelatorioIAAsync()
        {
            if (IsGerarRelatorio) return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Gerar Relatório com IA",
                "Deseja gerar um relatório personalizado dos últimos 30 dias com análise de IA?\n\nIsto pode demorar alguns segundos.",
                "Sim", "Não");

            if (!confirmar) return;

            try
            {
                IsGerarRelatorio = true;

                var relatorio = await _apiService.GerarMeuRelatorioAsync(30);

                if (relatorio != null)
                {
                    RelatorioPeriodo = relatorio.Periodo;
                    RelatorioMediaHumor = relatorio.MediaHumor;
                    RelatorioMediaEnergia = relatorio.MediaEnergia;
                    RelatorioMediaStress = relatorio.MediaStress;
                    RelatorioTendenciasHumor = relatorio.TendenciasHumor;
                    RelatorioTendenciasEnergia = relatorio.TendenciasEnergia;
                    RelatorioTendenciasStress = relatorio.TendenciasStress;
                    RelatorioRecomendacoes = relatorio.Recomendacoes;
                    RelatorioCompleto = relatorio.RelatorioCompleto;

                    MostrarRelatorio = true;

                    await Shell.Current.DisplayAlert(
                        "Sucesso",
                        relatorio.Mensagem,
                        "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await Shell.Current.DisplayAlert(
                    "Erro de Conexão",
                    "Não foi possível conectar ao servidor. Verifique sua conexão com a internet.",
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Não foi possível gerar o relatório: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsGerarRelatorio = false;
            }
        }

        [RelayCommand]
        public void FecharRelatorio()
        {
            MostrarRelatorio = false;
        }
    }
}