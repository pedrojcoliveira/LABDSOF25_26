using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Psicologo;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

public partial class DashboardPsicologoViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    public DashboardPsicologoViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        Alertas = new ObservableCollection<AlertaDto>();
        PacientesRecentes = new ObservableCollection<PacienteRecenteDto>();
    }

    #region Propriedades - Estatísticas

    [ObservableProperty]
    private int totalPacientesAtivos;

    [ObservableProperty]
    private int sessoesEsteMes;

    [ObservableProperty]
    private int sessoesEstaSemana;

    [ObservableProperty]
    private int mensagensNaoLidas;

    [ObservableProperty]
    private int relatoriosNaoLidos;

    [ObservableProperty]
    private int tarefasPendentes;

    #endregion

    #region Propriedades - Gráfico

    [ObservableProperty]
    private Chart graficoSessoes;

    #endregion

    #region Propriedades - Alertas e Pacientes

    [ObservableProperty]
    private ObservableCollection<AlertaDto> alertas;

    [ObservableProperty]
    private ObservableCollection<PacienteRecenteDto> pacientesRecentes;

    #endregion

    #region Propriedades - UI State

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    [ObservableProperty]
    private bool temDados = true;

    [ObservableProperty]
    private bool temAlertas;

    #endregion

    /// <summary>
    /// Carrega todos os dados do dashboard
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        MensagemErro = string.Empty;
        TemDados = true;

        try
        {
            Console.WriteLine("========================================");
            Console.WriteLine("🔍 A iniciar carregamento do Dashboard Psicólogo");

            // 1. Verificar autenticação
            if (!await _authService.IsAuthenticatedAsync())
            {
                Console.WriteLine("❌ Não autenticado!");
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // 2. Verificar role
            var role = await _authService.GetUserRoleAsync();
            Console.WriteLine($"🔍 Role: {role}");

            if (role != "Psicologo")
            {
                MensagemErro = "Acesso negado. Apenas psicólogos podem aceder a este dashboard.";
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // 3. Carregar dados da API
            var dashboard = await _apiService.GetDashboardPsicologoAsync();

            if (dashboard == null)
            {
                Console.WriteLine("❌ Dashboard é null!");
                MensagemErro = "Erro ao carregar dados.";
                TemDados = false;
                return;
            }

            Console.WriteLine($"✅ Dashboard recebido:");
            Console.WriteLine($"   Pacientes Ativos: {dashboard.TotalPacientesAtivos}");
            Console.WriteLine($"   Sessões Este Mês: {dashboard.SessoesEsteMes}");
            Console.WriteLine($"   Mensagens Não Lidas: {dashboard.MensagensNaoLidas}");

            // 4. Preencher estatísticas
            TotalPacientesAtivos = dashboard.TotalPacientesAtivos;
            SessoesEsteMes = dashboard.SessoesEsteMes;
            SessoesEstaSemana = dashboard.SessoesEstaSemana;
            MensagensNaoLidas = dashboard.MensagensNaoLidas;
            RelatoriosNaoLidos = dashboard.RelatoriosNaoLidos;
            TarefasPendentes = dashboard.TarefasPendentes;

            // 5. Criar gráfico de sessões
            if (dashboard.SessoesPorDia != null && dashboard.SessoesPorDia.Any())
            {
                var entries = dashboard.SessoesPorDia.Select(s => new ChartEntry(s.TotalSessoes)
                {
                    Label = s.DiaSemana,
                    ValueLabel = s.TotalSessoes.ToString(),
                    Color = SKColor.Parse("#10B981")
                }).ToList();

                GraficoSessoes = new BarChart
                {
                    Entries = entries,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 32,
                    ValueLabelOrientation = Orientation.Horizontal,
                    MinValue = 0
                };

                Console.WriteLine($"✅ Gráfico criado com {entries.Count} dias");
            }
            else
            {
                Console.WriteLine("⚠️ Sem dados de sessões");
            }

            // 6. Preencher alertas
            Alertas.Clear();
            if (dashboard.Alertas != null && dashboard.Alertas.Any())
            {
                foreach (var alerta in dashboard.Alertas)
                {
                    Alertas.Add(alerta);
                }
                TemAlertas = true;
                Console.WriteLine($"✅ {Alertas.Count} alertas adicionados");
            }
            else
            {
                TemAlertas = false;
            }

            // 7. Preencher pacientes recentes
            PacientesRecentes.Clear();
            if (dashboard.PacientesRecentes != null && dashboard.PacientesRecentes.Any())
            {
                foreach (var paciente in dashboard.PacientesRecentes)
                {
                    PacientesRecentes.Add(paciente);
                }
                Console.WriteLine($"✅ {PacientesRecentes.Count} pacientes recentes");
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

    /// <summary>
    /// Navega para a lista de pacientes
    /// </summary>
    [RelayCommand]
    private async Task IrParaPacientesAsync()
    {
        await Shell.Current.GoToAsync("//ListaPacientesPage");
    }

    /// <summary>
    /// Navega para mensagens
    /// </summary>
    [RelayCommand]
    private async Task IrParaMensagensAsync()
    {
        await Shell.Current.GoToAsync("//ConversasPsicologo");
    }

    /// <summary>
    /// Navega para o perfil de um paciente
    /// </summary>
    [RelayCommand]
    private async Task AbrirPerfilPacienteAsync(PacienteRecenteDto paciente)
    {
        if (paciente == null) return;

        try
        {
            await Shell.Current.GoToAsync($"DetalhePacientePage?pacienteId={paciente.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro",
                $"Erro ao abrir perfil: {ex.Message}", "OK");
        }
    }
}