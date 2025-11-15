using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.ModelViews
{
    public partial class RelatorioIAModelView : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly Guid _pacienteId = Guid.Parse("d1f3a2f3-2348-464d-ae76-533be243d50a");

        public RelatorioIAModelView(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ← LOWERCASE com underscore para o MVVM Toolkit gerar "Relatorios" (público)
        [ObservableProperty]
        private ObservableCollection<RelatorioListaDto> relatorios = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool temRelatorios;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        public async Task LoadRelatoriosAsync()
        {
            IsLoading = true;
            MensagemErro = string.Empty;

            try
            {
                var url = $"api/RelatorioIA/paciente/{_pacienteId}";
                var fullUrl = $"{_httpClient.BaseAddress}{url}";

                Console.WriteLine("========================================");
                Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
                Console.WriteLine($"URL Completa: {fullUrl}");
                Console.WriteLine("========================================");

                var lista = await _httpClient.GetFromJsonAsync<List<RelatorioListaDto>>(url);

                Console.WriteLine($"SUCESSO! Relatórios recebidos: {lista?.Count ?? 0}");

                Relatorios.Clear();
                if (lista != null && lista.Any())
                {
                    foreach (var relatorio in lista)
                    {
                        Relatorios.Add(relatorio);
                    }
                    TemRelatorios = true;
                }
                else
                {
                    TemRelatorios = false;
                    MensagemErro = "Nenhum relatório encontrado";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                MensagemErro = $"Erro ao carregar relatórios: {ex.Message}";
                TemRelatorios = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task VerDetalhes(RelatorioListaDto relatorio)
        {
            await Shell.Current.GoToAsync($"relatoriodetalhes?relatorioId={relatorio.Id}");
        }

        [RelayCommand]
        private async Task GerarNovoRelatorio()
        {
            IsLoading = true;
            try
            {
                var response = await _httpClient.PostAsync($"api/RelatorioIA/gerar/{_pacienteId}", null);

                if (response.IsSuccessStatusCode)
                {
                    await LoadRelatoriosAsync();
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Relatório gerado com sucesso!", "OK");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao gerar relatório: {errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Erro: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}