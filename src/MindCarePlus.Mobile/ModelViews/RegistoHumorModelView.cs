using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Mobile.Services.Interfaces;
using MindCarePlus.Shared.DTOs;
using MindCarePlus.Shared.DTOs.RegistoDiario;

namespace MindCarePlus.Mobile.ViewModels
{
    public partial class RegistoHumorModelView : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private int selectedMood;

        [ObservableProperty]
        private int stressLevel = 3;

        [ObservableProperty]
        private int energyLevel = 3;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public bool CanSave => SelectedMood > 0 && !IsBusy;

        public RegistoHumorModelView(ApiService apiService, IDialogService dialogService)
        {
            _apiService = apiService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void SelectMood(string moodStr)
        {
            // 2. Converte a string para int
            if (int.TryParse(moodStr, out int moodId))
            {
                // 3. Só depois atribui o valor
                SelectedMood = moodId;
                OnPropertyChanged(nameof(CanSave));
            }
        }

        partial void OnStressLevelChanged(int value)
        {
            OnPropertyChanged(nameof(StressLevelText));
            OnPropertyChanged(nameof(StressColor));
        }

        partial void OnEnergyLevelChanged(int value)
        {
            OnPropertyChanged(nameof(EnergyLevelText));
            OnPropertyChanged(nameof(EnergyColor));
        }

        public string StressLevelText => StressLevel switch
        {
            <= 2 => "😌 Baixo",
            3 => "😐 Moderado",
            _ => "😰 Elevado"
        };

        public Color StressColor => StressLevel switch
        {
            <= 2 => Color.FromArgb("#22c55e"),
            3 => Color.FromArgb("#eab308"),
            _ => Color.FromArgb("#ef4444")
        };

        public string EnergyLevelText => EnergyLevel switch
        {
            <= 2 => "😴 Baixa",
            3 => "😊 Moderada",
            _ => "⚡ Alta"
        };

        public Color EnergyColor => EnergyLevel switch
        {
            <= 2 => Color.FromArgb("#ef4444"),
            3 => Color.FromArgb("#eab308"),
            _ => Color.FromArgb("#22c55e")
        };

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveMoodAsync()
        {
            if (!CanSave) return;

            IsBusy = true;

            try
            {
                var registo = new CriarRegistoDiarioDto
                {
                    NivelHumor = SelectedMood,
                    NivelStress = StressLevel,
                    NivelEnergia = EnergyLevel,
                    Notas = Notes
                };

                var resultado = await _apiService.CriarRegistoAsync(registo);

                if (resultado != null)
                {
                    await _dialogService.ShowAlertAsync("Sucesso ✅",
                        "Registo guardado com sucesso!", "OK");

                    // Reset form
                    SelectedMood = 0;
                    StressLevel = 3;
                    EnergyLevel = 3;
                    Notes = string.Empty;
                    OnPropertyChanged(nameof(CanSave));
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Erro ❌",
                        "Não foi possível guardar o registo. Verifica se a API está a correr.", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Erro ❌",
                    $"Erro: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            await _dialogService.ShowAlertAsync("Info",
                "Funcionalidade de histórico será implementada em breve!", "OK");
        }
    }
}