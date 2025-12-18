// Mobile/ViewModels/MinhasSessoesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ViewModels
{
    public partial class MinhasSessoesViewModel : ObservableObject
    {
        private readonly SessaoService _sessaoService;

        [ObservableProperty]
        private ObservableCollection<SessaoDto> sessoes = new();

        [ObservableProperty]
        private ObservableCollection<SessaoDto> sessoesFiltradas = new();

        [ObservableProperty]
        private string filtroEstado = "Todas";

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool temSessoes;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        [ObservableProperty]
        private int totalPendentes;

        [ObservableProperty]
        private int totalAgendadas;

        [ObservableProperty]
        private int totalConcluidas;

        public ObservableCollection<string> Filtros { get; } = new()
        {
            "Todas",
            "Pendente",
            "Agendada",
            "Rejeitada",
            "Cancelada",
            "Concluída"
        };

        public MinhasSessoesViewModel(SessaoService sessaoService)
        {
            _sessaoService = sessaoService;
        }

        [RelayCommand]
        public async Task CarregarSessoesAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            MensagemErro = string.Empty;

            try
            {
                var listaSessoes = await _sessaoService.GetMinhasSolicitacoesAsync();

                Sessoes.Clear();
                SessoesFiltradas.Clear();

                if (listaSessoes != null && listaSessoes.Any())
                {
                    foreach (var sessao in listaSessoes)
                    {
                        Sessoes.Add(sessao);
                    }

                    AtualizarContadores();
                    AplicarFiltro();
                }

                TemSessoes = SessoesFiltradas.Count > 0;
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao carregar sessões: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task RefrescarSessoesAsync()
        {
            if (IsRefreshing) return;

            IsRefreshing = true;
            try
            {
                await CarregarSessoesAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public void FiltrarPorEstado(string estado)
        {
            FiltroEstado = estado;
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            SessoesFiltradas.Clear();

            var query = Sessoes.AsEnumerable();

            if (FiltroEstado != "Todas")
            {
                query = query.Where(s => s.Estado == FiltroEstado);
            }

            foreach (var sessao in query)
            {
                SessoesFiltradas.Add(sessao);
            }

            TemSessoes = SessoesFiltradas.Count > 0;
        }

        private void AtualizarContadores()
        {
            TotalPendentes = Sessoes.Count(s => s.Estado == "Pendente");
            TotalAgendadas = Sessoes.Count(s => s.Estado == "Agendada");
            TotalConcluidas = Sessoes.Count(s => s.Estado == "Concluida");
        }

        [RelayCommand]
        public async Task VerDetalhesSessaoAsync(SessaoDto sessao)
        {
            if (sessao == null) return;

            try
            {
                await Shell.Current.DisplayAlert(
                    "Detalhes da Sessão",
                    $"Psicólogo: {sessao.NomePsicologo}\n" +
                    $"Especialidade: {sessao.EspecialidadePsicologo}\n" +
                    $"Data: {sessao.DataHoraFormatada}\n" +
                    $"Duração: {sessao.Duracao.TotalMinutes} minutos\n" +
                    $"Estado: {sessao.EstadoTexto}\n\n" +
                    $"Notas: {sessao.NotasPaciente ?? "Sem notas"}",
                    "Fechar");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao exibir detalhes: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task EntrarNaSessaoAsync(SessaoDto sessao)
        {
            if (sessao == null || string.IsNullOrEmpty(sessao.LinkSessao)) return;

            try
            {
                await Browser.Default.OpenAsync(sessao.LinkSessao, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir link: {ex.Message}", "OK");
            }
        }
    }
}