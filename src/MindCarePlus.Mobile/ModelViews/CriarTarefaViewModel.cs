using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.TarefaTerapeutica;

namespace MindCarePlus.Mobile.ModelViews;

[QueryProperty(nameof(PacienteId), "pacienteId")]
[QueryProperty(nameof(PacienteNome), "pacienteNome")]
public partial class CriarTarefaViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string pacienteId = string.Empty;

    [ObservableProperty]
    private string pacienteNome = string.Empty;

    [ObservableProperty]
    private string titulo = string.Empty;

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private DateTime prazo = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private int pontosRecompensa = 20;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    public DateTime DataMinima => DateTime.Today.AddDays(1);

    public CriarTarefaViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task CriarTarefaAsync()
    {
        if (IsLoading) return;

        MensagemErro = string.Empty;

        // Validações
        if (string.IsNullOrWhiteSpace(Titulo))
        {
            MensagemErro = "O título é obrigatório";
            return;
        }

        if (Titulo.Length < 3 || Titulo.Length > 100)
        {
            MensagemErro = "O título deve ter entre 3 e 100 caracteres";
            return;
        }

        if (string.IsNullOrWhiteSpace(Descricao))
        {
            MensagemErro = "A descrição é obrigatória";
            return;
        }

        if (Descricao.Length < 10 || Descricao.Length > 500)
        {
            MensagemErro = "A descrição deve ter entre 10 e 500 caracteres";
            return;
        }

        if (Prazo <= DateTime.Today)
        {
            MensagemErro = "O prazo deve ser uma data futura";
            return;
        }

        if (!Guid.TryParse(PacienteId, out Guid pacienteGuid))
        {
            MensagemErro = "ID do paciente inválido";
            return;
        }

        IsLoading = true;

        try
        {
            var dto = new CriarTarefaTerapeuticaDto
            {
                Titulo = Titulo.Trim(),
                Descricao = Descricao.Trim(),
                Prazo = Prazo.ToUniversalTime(),
                PontosRecompensa = PontosRecompensa,
                PacienteId = pacienteGuid
            };

            var resultado = await _apiService.CriarTarefaTerapeuticaAsync(dto);

            if (resultado != null)
            {
                await Shell.Current.DisplayAlert(
                    "Sucesso ✅",
                    $"Tarefa '{resultado.Titulo}' criada com sucesso!",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
            else
            {
                MensagemErro = "Erro ao criar tarefa. Verifique os dados e tente novamente.";
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro", MensagemErro, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}