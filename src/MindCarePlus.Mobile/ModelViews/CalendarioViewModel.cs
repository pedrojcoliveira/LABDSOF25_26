using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

public partial class CalendarioViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public CalendarioViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Sessoes = new ObservableCollection<SessaoCalendarioDto>();

        // Inicializar filtros
        DataInicio = DateTime.Today.AddMonths(-1);
        DataFim = DateTime.Today.AddMonths(1);
    }

    [ObservableProperty]
    private ObservableCollection<SessaoCalendarioDto> sessoes;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    [ObservableProperty]
    private DateTime dataInicio;

    [ObservableProperty]
    private DateTime dataFim;

    [ObservableProperty]
    private string filtroEstado = "Todas";

    [ObservableProperty]
    private int totalSessoes;

    [ObservableProperty]
    private int sessoesConfirmadas;

    [ObservableProperty]
    private int sessoesAgendadas;

    [ObservableProperty]
    private int sessoesCanceladas;

    [ObservableProperty]
    private bool temSessoes;

    [RelayCommand]
    public async Task CarregarSessoesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        MensagemErro = string.Empty;

        try
        {
            var estado = FiltroEstado == "Todas" ? null : FiltroEstado;

            var lista = await _apiService.GetMinhasSessoesAsync(
                DataInicio,
                DataFim,
                null,
                estado);

            Sessoes.Clear();
            foreach (var sessao in lista)
            {
                Sessoes.Add(sessao);
            }

            // Calcular estatísticas
            TotalSessoes = Sessoes.Count;
            SessoesConfirmadas = Sessoes.Count(s => s.Estado == "Confirmada");
            SessoesAgendadas = Sessoes.Count(s => s.Estado == "Agendada");
            SessoesCanceladas = Sessoes.Count(s => s.Estado == "Cancelada");

            TemSessoes = Sessoes.Any(); 
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar sessões: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AplicarFiltrosAsync()
    {
        await CarregarSessoesAsync();
    }

    [RelayCommand]
    public async Task VerDetalhesSessaoAsync(SessaoCalendarioDto sessao)
    {
        if (sessao == null) return;

        await Shell.Current.DisplayAlert(
            "Detalhes da Sessão",
            $"Paciente: {sessao.PacienteNome}\n" +
            $"Data: {sessao.DataFormatada} às {sessao.HoraFormatada}\n" +
            $"Duração: {sessao.DuracaoFormatada}\n" +
            $"Estado: {sessao.Estado}\n" +
            $"Notas: {sessao.Notas}",
            "Fechar");
    }

    [RelayCommand]
    public async Task AlterarEstadoSessaoAsync(SessaoCalendarioDto sessao)
    {
        if (sessao == null) return;

        var opcoes = new[] { "Confirmada", "Agendada", "Realizada", "Cancelada", "Faltou" };
        var escolha = await Shell.Current.DisplayActionSheet(
            "Alterar Estado",
            "Cancelar",
            null,
            opcoes);

        if (escolha == "Cancelar" || escolha == null) return;

        try
        {
            IsLoading = true;
            await _apiService.AtualizarEstadoSessaoAsync(sessao.Id, escolha);

            sessao.Estado = escolha;

            await CarregarSessoesAsync();

            await Shell.Current.DisplayAlert("Sucesso", "Estado atualizado!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }

    }

    [RelayCommand]
    public async Task AbrirLinkSessaoAsync(string linkSessao)
    {
        if (string.IsNullOrEmpty(linkSessao)) return;

        try
        {
            // Abre o link no browser padrão
            await Launcher.OpenAsync(new Uri(linkSessao));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                $"Não foi possível abrir o link: {ex.Message}",
                "OK");
        }
    }
}