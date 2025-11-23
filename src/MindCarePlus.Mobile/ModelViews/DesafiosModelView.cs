using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ViewModels
{
    public partial class DesafiosModelView : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<DesafioAtribuidoDto> desafios = new();

        [ObservableProperty]
        private int totalDesafios;

        [ObservableProperty]
        private int desafiosConcluidos;

        [ObservableProperty]
        private int pontosTotais;

        [ObservableProperty]
        private double progressoPercentagem;

        [ObservableProperty]
        private bool completouSemana;

        [ObservableProperty]
        private bool isBusy;

        public DesafiosModelView(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task CarregarDesafiosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                await InternalCarregarDadosAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar desafios: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConcluirDesafioAsync(DesafioAtribuidoDto desafio)
        {
            if (desafio.Estado == "Concluido")
            {
                await Shell.Current.DisplayAlert("Info", "Este desafio já foi concluído!", "OK");
                return;
            }

            IsBusy = true;

            string? mensagemErro = null;
            string mensagemSucesso = null;

            try
            {
                var response = await _apiService.ConcluirDesafioAsync(desafio.Id);

                if (response != null && response.Sucesso)
                {
                    // ***** AQUI ESTÁ A CORREÇÃO *****
                    // Chama o método INTERNO para recarregar os dados,
                    // ignorando a verificação do IsBusy.
                    await InternalCarregarDadosAsync();

                    // Prepara a mensagem de sucesso
                    mensagemSucesso = response.Mensagem;
                    if (response.BadgeGanha != null && response.BadgeGanha.Badge != null)
                    {
                        mensagemSucesso += $"\n\n🏆 Badge Conquistada:\n{response.BadgeGanha.Badge.ImagemUrl} {response.BadgeGanha.Badge.Nome} - Nível {response.BadgeGanha.Nivel}";
                    }
                    mensagemSucesso += $"\n\n⭐ +{response.PontosGanhos} pontos!";
                }
                else
                {
                    mensagemErro = "Não foi possível concluir o desafio.";
                }
            }
            catch (Exception ex)
            {
                mensagemErro = $"Erro: {ex.Message}";
            }
            finally
            {
                // A UI atualiza ANTES do popup
                IsBusy = false;
            }

            // O popup é mostrado DEPOIS da UI estar atualizada
            if (mensagemSucesso != null)
            {
                await Shell.Current.DisplayAlert("Parabéns! 🎉", mensagemSucesso, "OK");
            }
            else if (mensagemErro != null)
            {
                await Shell.Current.DisplayAlert("Erro", mensagemErro, "OK");
            }
        }

        private async Task InternalCarregarDadosAsync()
        {


            // 1. Buscar o Progresso Semanal (Os 7 desafios do sistema)
            var progresso = await _apiService.GetMyDesafiosSemanaisAsync(); // Já usa o token, não precisa de ID

            // 2. Buscar as Tarefas Terapêuticas (Do Psicólogo)
            var tarefasTerapeuticas = await _apiService.ListarMinhasTarefasTerapeuticasAsync();

            if (progresso != null)
            {
                var listaTodosDesafios = new List<DesafioAtribuidoDto>(progresso.Desafios);

                // --- CORREÇÃO 1: CALCULAR O PROGRESSO ANTES DE MISTURAR ---
                // A barra de progresso e o "CompletouSemana" devem depender APENAS dos desafios semanais
                // (Ignoramos as tarefas terapêuticas para este cálculo)

                int semanaisFeitos = progresso.Desafios.Count(d => d.Estado == "Concluido" && d.Desafio.Tipo != "TarefaTerapeutica");

                DesafiosConcluidos = semanaisFeitos;
                TotalDesafios = 7; // A meta semanal é sempre 7

                // Barra de progresso (0 a 1)
                ProgressoPercentagem = (double)semanaisFeitos / 7.0;

                // Só mostra "Semana Completa" se os 7 oficiais estiverem feitos
                CompletouSemana = semanaisFeitos >= 7;

                // O total de pontos soma tudo (Semanais + Terapêuticos)
                // (Assumindo que a API já devolve os pontos totais do user, isto fica igual)
                PontosTotais = progresso.PontosTotais;


                // --- CORREÇÃO 2: ADICIONAR TAREFAS À LISTA VISUAL ---
                if (tarefasTerapeuticas != null)
                {
                    foreach (var tarefa in tarefasTerapeuticas)
                    {
                        // Só adicionamos se não estiver já na lista (evita duplicados)
                        if (!listaTodosDesafios.Any(d => d.Id == tarefa.Id))
                        {
                            listaTodosDesafios.Add(new DesafioAtribuidoDto
                            {
                                Id = tarefa.Id,
                                Desafio = new DesafioDto
                                {
                                    Titulo = tarefa.Titulo,
                                    Descricao = tarefa.Descricao,
                                    PontosRecompensa = tarefa.PontosRecompensa,
                                    Tipo = "TarefaTerapeutica", // Importante para distinguir
                                    Icone = "📋",
                                    BadgeNome = tarefa.PsicologoNome ?? "Psicólogo",
                                    BadgeIcone = "👨‍⚕️"
                                },
                                Estado = tarefa.Estado,
                                DataAtribuicao = tarefa.DataAtribuicao,
                                DataConclusao = tarefa.DataConclusao,
                                DiaSemana = "Tarefa Extra" // Para não aparecer "Monday" etc
                            });
                        }
                    }
                }

                // 3. Atualizar a lista que aparece no ecrã
                // Ordenamos: Pendentes primeiro, depois por data
                var listaOrdenada = listaTodosDesafios
                    .OrderBy(d => d.Estado == "Concluido") // Pendentes (false) primeiro
                    .ThenBy(d => d.DataAtribuicao);

                Desafios = new ObservableCollection<DesafioAtribuidoDto>(listaOrdenada);
            }
        }
    }
}
