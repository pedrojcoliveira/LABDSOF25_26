using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Shared.DTOs;
using MindCarePlus.Mobile.Services;

namespace MindCarePlus.Mobile.ViewModels
{
    [QueryProperty(nameof(Psicologo), "psicologo")]
    public partial class SolicitarSessaoViewModel : ObservableObject
    {
        private readonly SessaoService _sessaoService;

        [ObservableProperty]
        private PsicologoContactoDto? psicologo; // ← MUDOU de PsychologistDto para PsicologoContactoDto

        [ObservableProperty]
        private DateTime dataSelecionada = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private TimeSpan horaSelecionada = new TimeSpan(10, 0, 0);

        [ObservableProperty]
        private string notasPaciente = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        public string NomePsicologo => Psicologo?.Nome ?? string.Empty; // ← MUDOU de Name para Nome
        public string EspecialidadePsicologo => Psicologo?.Especialidade ?? string.Empty; // ← MUDOU de Specialty para Especialidade

        public SolicitarSessaoViewModel(SessaoService sessaoService)
        {
            _sessaoService = sessaoService;
        }

        [RelayCommand]
        private async Task SolicitarSessao()
        {
            System.Diagnostics.Debug.WriteLine("🔵 Botão clicado!");

            if (IsLoading || Psicologo == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Bloqueado: IsLoading={IsLoading}, Psicologo={Psicologo?.Nome ?? "NULL"}");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("🟢 Iniciando processo...");
                IsLoading = true;
                MensagemErro = string.Empty;

                if (DataSelecionada < DateTime.Today)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Data inválida");
                    await Application.Current!.MainPage!.DisplayAlert("Erro", "A data deve ser futura", "OK");
                    return;
                }

                var dataHoraMarcada = DataSelecionada.Date + HoraSelecionada;
                System.Diagnostics.Debug.WriteLine($"📅 Data/Hora: {dataHoraMarcada}");

                if (dataHoraMarcada <= DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Data/hora no passado");
                    await Application.Current!.MainPage!.DisplayAlert("Erro", "A data e hora devem ser no futuro", "OK");
                    return;
                }

                var request = new SolicitarSessaoDto
                {
                    PsicologoId = Psicologo.Id,
                    DataMarcada = DataSelecionada,
                    HoraMarcada = HoraSelecionada,
                    NotasPaciente = string.IsNullOrWhiteSpace(NotasPaciente) ? null : NotasPaciente.Trim()
                };

                System.Diagnostics.Debug.WriteLine($"📤 Enviando pedido: {request.PsicologoId}");
                var resultado = await _sessaoService.SolicitarSessaoAsync(request);

                if (resultado != null)
                {
                    await Shell.Current.GoToAsync("//minhassessoes");
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ HttpRequestException: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Erro",
                    ex.Message.Contains("já está reservado")
                        ? "Este horário já está reservado. Por favor, escolha outro."
                        : $"Erro ao enviar pedido: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                await Application.Current!.MainPage!.DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("🔴 Finally - IsLoading = false");
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Cancelar()
        {
            await Shell.Current.GoToAsync("..");
        }

        partial void OnPsicologoChanged(PsicologoContactoDto? value)
    {
        System.Diagnostics.Debug.WriteLine($"🔍 Psicólogo recebido: {value?.Nome ?? "NULL"}");
        OnPropertyChanged(nameof(NomePsicologo));
        OnPropertyChanged(nameof(EspecialidadePsicologo));
    }
    }
}