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
            // TODO: Pegar ID do utilizador autenticado
            var pacienteId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

            var progresso = await _apiService.GetDesafiosSemanaisAsync(pacienteId);

            if (progresso != null)
            {
                Desafios = new ObservableCollection<DesafioAtribuidoDto>(progresso.Desafios);
                TotalDesafios = progresso.TotalDesafios;
                DesafiosConcluidos = progresso.DesafiosConcluidos;
                PontosTotais = progresso.PontosTotais;
                ProgressoPercentagem = progresso.ProgressoPercentagem;
                CompletouSemana = progresso.CompletouSemana;
            }
        }
    }
}
