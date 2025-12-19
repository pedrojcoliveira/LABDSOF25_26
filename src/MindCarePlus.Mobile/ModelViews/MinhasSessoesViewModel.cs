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
        private readonly AvaliacaoService _avaliacaoService;

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

        [ObservableProperty]
        private SessaoDto? sessaoParaAvaliar;

        [ObservableProperty]
        private int pontuacaoSelecionada = 5;

        [ObservableProperty]
        private string comentarioAvaliacao = string.Empty;

        [ObservableProperty]
        private bool modalAvaliacaoVisivel;


        public ObservableCollection<string> Filtros { get; } = new()
        {
            "Todas",
            "Pendente",
            "Agendada",
            "Rejeitada",
            "Cancelada",
            "Concluída"
        };

        public MinhasSessoesViewModel(SessaoService sessaoService, AvaliacaoService avaliacaoService)
        {
            _sessaoService = sessaoService;
            _avaliacaoService = avaliacaoService;
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

        [RelayCommand]
        public async Task CancelarSessaoAsync(SessaoDto sessao)
        {
            if (sessao == null) return;

            try
            {
                // Verificar se pode cancelar
                if (sessao.Estado != "Pendente" && sessao.Estado != "Agendada")
                {
                    await Shell.Current.DisplayAlert(
                        "Não permitido",
                        $"Não é possível cancelar uma sessão com estado '{sessao.EstadoTexto}'",
                        "OK");
                    return;
                }

                // Confirmar cancelamento
                bool confirmar = await Shell.Current.DisplayAlert(
                    "Cancelar Sessão",
                    $"Tem certeza que deseja cancelar a sessão com {sessao.NomePsicologo} no dia {sessao.DataHoraFormatada}?",
                    "Sim, cancelar",
                    "Não");

                if (!confirmar) return;

                IsLoading = true;

                var sucesso = await _sessaoService.CancelarSessaoAsync(sessao.Id);

                if (sucesso)
                {
                    sessao.Estado = "Cancelada";
                    sessao.DataAtualizacao = DateTime.UtcNow;

                    // Atualizar contadores
                    AtualizarContadores();

                    // Reaplicar filtro
                    AplicarFiltro();

                    await Shell.Current.DisplayAlert(
                        "Sucesso",
                        "Sessão cancelada com sucesso",
                        "OK");

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500); // Pequeno delay
                        await CarregarSessoesAsync();
                    });
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Erro",
                        "Não foi possível cancelar a sessão. Tente novamente.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao cancelar sessão: {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Ocorreu um erro ao cancelar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AbrirModalAvaliacaoAsync(SessaoDto sessao)
        {
            if (sessao == null) return;

            try
            {
                // Verificar se pode avaliar
                if (sessao.Estado != "Concluida")
                {
                    await Shell.Current.DisplayAlert(
                        "Não permitido",
                        "Apenas sessões concluídas podem ser avaliadas",
                        "OK");
                    return;
                }

                // Verificar se já foi avaliada
                var avaliacaoExistente = await _avaliacaoService.GetAvaliacaoPorSessaoAsync(sessao.Id);
                if (avaliacaoExistente != null)
                {
                    await Shell.Current.DisplayAlert(
                        "Já avaliada",
                        $"Esta sessão já foi avaliada com {avaliacaoExistente.Pontuacao} ⭐\n\n{avaliacaoExistente.Comentario}",
                        "OK");
                    return;
                }

                // Preparar modal
                SessaoParaAvaliar = sessao;
                PontuacaoSelecionada = 5;
                ComentarioAvaliacao = string.Empty;
                ModalAvaliacaoVisivel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao abrir modal: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task SubmeterAvaliacaoAsync()
        {
            if (SessaoParaAvaliar == null) return;

            try
            {
                IsLoading = true;

                var request = new CriarAvaliacaoDto
                {
                    SessaoId = SessaoParaAvaliar.Id,
                    Pontuacao = PontuacaoSelecionada,
                    Comentario = string.IsNullOrWhiteSpace(ComentarioAvaliacao) ? null : ComentarioAvaliacao
                };

                var resultado = await _avaliacaoService.CriarAvaliacaoAsync(request);

                if (resultado != null)
                {
                    ModalAvaliacaoVisivel = false;

                    await Shell.Current.DisplayAlert(
                        "Sucesso",
                        $"Avaliação de {PontuacaoSelecionada} ⭐ enviada com sucesso!",
                        "OK");

                    // Recarregar lista
                    await CarregarSessoesAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Erro",
                        "Não foi possível enviar a avaliação. Tente novamente.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao submeter avaliação: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void FecharModalAvaliacao()
        {
            ModalAvaliacaoVisivel = false;
            SessaoParaAvaliar = null;
        }
    }
}