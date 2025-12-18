using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Paciente;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.ModelViews;

public partial class DetalhePacienteViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // 1. Ler o ID do Paciente
        if (query.ContainsKey("pacienteId"))
        {
            var novoId = query["pacienteId"].ToString();

            // Só carrega se o ID for diferente (evita recarregar se já lá estivermos)
            if (PacienteId != novoId)
            {
                PacienteId = novoId;
                // Dispara o carregamento (Fire-and-forget)
                Task.Run(async () => await CarregarDetalhesAsync());
            }
        }

        // 2. Ler a Tab desejada
        if (query.ContainsKey("tab"))
        {
            var tab = query["tab"].ToString();
            if (tab == "relatorios")
            {
                // Índice 3 corresponde à aba Relatórios (conforme o teu código: 0=Dados, 1=Humor, 2=Sessões, 3=Relatórios)
                TabSelecionada = 3;
            }
        }
        else
        {
            // Se não especificar tab, vai para a inicial (Dados)
            TabSelecionada = 0;
        }
    }
    public DetalhePacienteViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        HistoricoHumor = new ObservableCollection<RegistoHumorDto>();
        Sessoes = new ObservableCollection<SessaoResumoDto>();
        RelatoriosIA = new ObservableCollection<RelatorioIAResumoDto>();
    }


    #region Propriedades - Navegação

    [ObservableProperty]
    private string pacienteId = string.Empty;

    #endregion

    #region Propriedades - Dados do Paciente


    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private int idade;

    [ObservableProperty]
    private string? objectivo;

    [ObservableProperty]
    private int pontos;

    [ObservableProperty]
    private int nivelGamificacao;

    [ObservableProperty]
    private DateTime dataRegisto;

    #endregion

    #region Propriedades - Histórico e Sessões

    [ObservableProperty]
    private ObservableCollection<RegistoHumorDto> historicoHumor;

    [ObservableProperty]
    private ObservableCollection<SessaoResumoDto> sessoes;

    [ObservableProperty]
    private ObservableCollection<RelatorioIAResumoDto> relatoriosIA;

    #endregion

    #region Propriedades - Estatísticas

    [ObservableProperty]
    private int totalRegistos;

    [ObservableProperty]
    private int totalSessoes;

    [ObservableProperty]
    private int totalRelatorios;

    [ObservableProperty]
    private double mediaHumor;

    [ObservableProperty]
    private double mediaStress;

    [ObservableProperty]
    private double mediaEnergia;

    [ObservableProperty]
    private int diasConsecutivos;

    #endregion

    #region Propriedades - UI State

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    [ObservableProperty]
    private int tabSelecionada = 0; // 0=Dados, 1=Humor, 2=Sessões, 3=Relatórios

    [ObservableProperty]
    private bool temHistorico;

    [ObservableProperty]
    private bool temSessoes;

    [ObservableProperty]
    private bool temRelatorios;

    #endregion

    #region Propriedades - AI Report
    [ObservableProperty]
    private bool isGerandoRelatorio;
    #endregion

    #region Propriedades - Relatório Expandido

    [ObservableProperty]
    private RelatorioIAResumoDto? relatorioSelecionado;

    [ObservableProperty]
    private bool mostrarDetalheRelatorio;

    // Detalhes do relatório selecionado
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

    [ObservableProperty]
    private decimal relatorioMediaHumor;

    [ObservableProperty]
    private decimal relatorioMediaEnergia;

    [ObservableProperty]
    private decimal relatorioMediaStress;

    [ObservableProperty]
    private string relatorioPeriodo = string.Empty;

    #endregion



    /// <summary>
    /// Carrega os detalhes completos do paciente
    /// </summary>
    [RelayCommand]
    public async Task CarregarDetalhesAsync()
    {
        if (IsLoading) return;

        // Validar ID
        if (string.IsNullOrEmpty(PacienteId) || !Guid.TryParse(PacienteId, out Guid pacienteGuid))
        {
            MensagemErro = "ID de paciente inválido";
            return;
        }

        IsLoading = true;
        MensagemErro = string.Empty;

        try
        {
            // Verificar autenticação
            if (!await _authService.IsAuthenticatedAsync())
            {
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // Verificar role
            var role = await _authService.GetUserRoleAsync();
            if (role != "Psicologo")
            {
                MensagemErro = "Acesso negado. Apenas psicólogos podem aceder a esta área.";
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // Carregar detalhes da API
            var detalhes = await _apiService.ObterDetalhesPacienteAsync(pacienteGuid);

            if (detalhes == null)
            {
                MensagemErro = "Paciente não encontrado ou sem permissão de acesso.";
                return;
            }

            // Mapear dados pessoais
            Nome = detalhes.Nome;
            Email = detalhes.Email;
            Idade = detalhes.Idade;
            Objectivo = detalhes.Objectivo;
            Pontos = detalhes.Pontos;
            NivelGamificacao = detalhes.NivelGamificacao;
            DataRegisto = detalhes.DataRegisto;

            // Mapear histórico de humor
            HistoricoHumor.Clear();
            foreach (var registo in detalhes.HistoricoHumor)
            {
                HistoricoHumor.Add(registo);
            }
            TemHistorico = HistoricoHumor.Count > 0;

            // Mapear sessões
            Sessoes.Clear();
            foreach (var sessao in detalhes.Sessoes)
            {
                Sessoes.Add(sessao);
            }
            TemSessoes = Sessoes.Count > 0;

            // Mapear relatórios IA
            RelatoriosIA.Clear();
            foreach (var relatorio in detalhes.RelatoriosIA)
            {
                RelatoriosIA.Add(relatorio);
            }
            TemRelatorios = RelatoriosIA.Count > 0;

            // Mapear estatísticas
            TotalRegistos = detalhes.Estatisticas.TotalRegistos;
            TotalSessoes = detalhes.Estatisticas.TotalSessoes;
            TotalRelatorios = detalhes.Estatisticas.TotalRelatorios;
            MediaHumor = detalhes.Estatisticas.MediaHumor;
            MediaStress = detalhes.Estatisticas.MediaStress;
            MediaEnergia = detalhes.Estatisticas.MediaEnergia;
            DiasConsecutivos = detalhes.Estatisticas.DiasConsecutivos;
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar detalhes: {ex.Message}";
            Console.WriteLine($"Erro em CarregarDetalhesAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Muda para o tab de dados pessoais
    /// </summary>
    [RelayCommand]
    public void MostrarDados()
    {
        TabSelecionada = 0;
    }

    /// <summary>
    /// Muda para o tab de histórico de humor
    /// </summary>
    [RelayCommand]
    public void MostrarHistoricoHumor()
    {
        TabSelecionada = 1;
    }

    /// <summary>
    /// Muda para o tab de sessões
    /// </summary>
    [RelayCommand]
    public void MostrarSessoes()
    {
        TabSelecionada = 2;
    }

    /// <summary>
    /// Muda para o tab de relatórios IA
    /// </summary>
    [RelayCommand]
    public void MostrarRelatorios()
    {
        TabSelecionada = 3;
    }

    /// <summary>
    /// Volta para a lista de pacientes
    /// </summary>
    [RelayCommand]
    public async Task VoltarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Abre detalhes de uma sessão específica
    /// </summary>
    [RelayCommand]
    public async Task VerDetalhesSessaoAsync(SessaoResumoDto sessao)
    {
        if (sessao == null) return;

        await Shell.Current.DisplayAlert(
            "Detalhes da Sessão",
            $"Data: {sessao.DataSessao:dd/MM/yyyy HH:mm}\n" +
            $"Duração: {sessao.Duracao} minutos\n" +
            $"Notas: {sessao.Notas ?? "Sem notas"}\n" +
            $"Avaliação: {(sessao.Avaliada ? $"{sessao.NotaAvaliacao}/5" : "Não avaliada")}",
            "OK");
    }


    [RelayCommand]
    public async Task VerRelatorioDetalheAsync(RelatorioIAResumoDto relatorio)
    {
        if (relatorio == null) return;

        try
        {
            var detalhe = await _apiService.GetRelatorioDetalheAsync(relatorio.Id);

            if (detalhe != null)
            {
                relatorio.Lido = true;
                await Shell.Current.GoToAsync($"RelatorioDetalhe?relatorioId={relatorio.Id}");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task CarregarRelatoriosAsync()
    {
        if (string.IsNullOrEmpty(PacienteId)) return;
    
        if (!Guid.TryParse(PacienteId, out Guid pacienteGuid)) return;

        try
        {
            var relatorios = await _apiService.GetRelatoriosPacienteAsync(pacienteGuid);
            RelatoriosIA = new ObservableCollection<RelatorioIAResumoDto>(relatorios);
            TemRelatorios = RelatoriosIA.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar relatórios: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task GerarRelatorioIAAsync()
    {
        if (IsGerandoRelatorio || string.IsNullOrEmpty(PacienteId)) return;

        if (!Guid.TryParse(PacienteId, out Guid pacienteGuid)) return;

        bool confirmar = await Shell.Current.DisplayAlert(
            "Gerar Relatório IA",
            "Deseja gerar um novo relatório semanal com análise de IA?\n\nIsto pode demorar alguns segundos.",
            "Sim", "Não");

        if (!confirmar) return;

        try
        {
            IsGerandoRelatorio = true;

            var novoRelatorio = await _apiService.GerarRelatorioIAAsync(pacienteGuid);

            if (novoRelatorio != null)
            {
                RelatoriosIA.Insert(0, novoRelatorio);
                TemRelatorios = true;
                TotalRelatorios++;

                await Shell.Current.DisplayAlert(
                    "Sucesso",
                    "Relatório gerado com sucesso!",
                    "OK");

                // Expandir automaticamente o novo relatório
                await VerDetalhesRelatorioAsync(novoRelatorio);
            }
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
            IsGerandoRelatorio = false;
        }
    }

    [RelayCommand]
    public async Task VerDetalhesRelatorioAsync(RelatorioIAResumoDto relatorio)
    {
        if (relatorio == null) return;

        try
        {
            var detalhe = await _apiService.GetRelatorioDetalheAsync(relatorio.Id);

            if (detalhe != null)
            {
                relatorio.Lido = true;

                // USA MAIÚSCULAS - Propriedades, não campos!
                RelatorioSelecionado = relatorio;
                RelatorioPeriodo = $"{detalhe.PeriodoInicio} - {detalhe.PeriodoFim}";
                RelatorioMediaHumor = detalhe.MediaHumor;
                RelatorioMediaEnergia = detalhe.MediaEnergia;
                RelatorioMediaStress = detalhe.MediaStress;
                RelatorioTendenciasHumor = detalhe.TendenciasHumor;
                RelatorioTendenciasEnergia = detalhe.TendenciasEnergia;
                RelatorioTendenciasStress = detalhe.TendenciasStress;
                RelatorioRecomendacoes = detalhe.Recomendacoes;
                RelatorioCompleto = detalhe.RelatorioCompleto;
                MostrarDetalheRelatorio = true;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void FecharDetalheRelatorio()
    {
        MostrarDetalheRelatorio = false;  
        RelatorioSelecionado = null;
    }


    /// <summary>
    /// Navega para criar nova tarefa terapêutica
    /// </summary>
    [RelayCommand]
    public async Task CriarNovaTarefaAsync()
    {
        await Shell.Current.GoToAsync($"CriarTarefaPage?pacienteId={PacienteId}&pacienteNome={Nome}");
    }

}
