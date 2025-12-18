using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

public partial class ContactarPsicologoViewModel : ObservableObject
{
    private readonly PsicologoService _psicologoService;

    [ObservableProperty]
    private ObservableCollection<PsicologoContactoDto> psicologos = new();

    [ObservableProperty]
    private ObservableCollection<PsicologoContactoDto> psicologosFiltrados = new();

    [ObservableProperty]
    private string textoPesquisa = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool temPsicologos;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    [ObservableProperty]
    private string especialidadeSelecionada = "Todas";

    public ObservableCollection<string> Especialidades { get; } = new()
    {
        "Todas",
        "Psicologia Clínica",
        "Psicologia Infantil",
        "Psicologia Organizacional",
        "Neuropsicologia",
        "Psicoterapia",
        "Terapia Cognitivo-Comportamental"
    };

    public ContactarPsicologoViewModel(PsicologoService psicologoService)
    {
        _psicologoService = psicologoService ?? throw new ArgumentNullException(nameof(psicologoService));
    }

    [RelayCommand]
    public async Task CarregarPsicologosAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        MensagemErro = string.Empty;

        try
        {
            var especialidade = EspecialidadeSelecionada == "Todas" ? null : EspecialidadeSelecionada;

            Console.WriteLine($"🔍 Carregando psicólogos (especialidade: {especialidade ?? "todas"})");

            var psicologosLista = await _psicologoService.ListarPsicologosAsync(especialidade, true);

            Psicologos.Clear();
            PsicologosFiltrados.Clear();

            if (psicologosLista != null && psicologosLista.Any())
            {
                Console.WriteLine($"✅ {psicologosLista.Count} psicólogos encontrados");
                foreach (var psicologo in psicologosLista)
                {
                    Psicologos.Add(psicologo);
                    PsicologosFiltrados.Add(psicologo);
                }
            }
            else
            {
                Console.WriteLine("⚠️ Nenhum psicólogo encontrado");
            }

            TemPsicologos = PsicologosFiltrados.Count > 0;
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar psicólogos: {ex.Message}";
            Console.WriteLine($"❌ Erro em CarregarPsicologosAsync: {ex}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RefrescarPsicologosAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;

        try
        {
            await CarregarPsicologosAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public void PesquisarPsicologos()
    {
        PsicologosFiltrados.Clear();

        var query = Psicologos.AsEnumerable();

        // Filtrar por texto de pesquisa
        if (!string.IsNullOrWhiteSpace(TextoPesquisa))
        {
            query = query.Where(p =>
                p.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                p.Especialidade.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase));
        }

        // Filtrar por especialidade
        if (EspecialidadeSelecionada != "Todas")
        {
            query = query.Where(p => p.Especialidade == EspecialidadeSelecionada);
        }

        foreach (var psicologo in query)
        {
            PsicologosFiltrados.Add(psicologo);
        }

        TemPsicologos = PsicologosFiltrados.Count > 0;
    }

    [RelayCommand]
    public void LimparPesquisa()
    {
        TextoPesquisa = string.Empty;
        PesquisarPsicologos();
    }

    [RelayCommand]
    public async Task FiltrarPorEspecialidadeAsync()
    {
        await CarregarPsicologosAsync();
    }

    [RelayCommand]
    public async Task VerPerfilAsync(PsicologoContactoDto psicologo)
    {
        if (psicologo == null) return;

        try
        {
            await Shell.Current.DisplayAlert(
                "Perfil",
                $"Nome: {psicologo.Nome}\n" +
                $"Especialidade: {psicologo.Especialidade}\n" +
                $"Cédula: {psicologo.NumeroCedulaProfissional}\n" +
                $"Pacientes: {psicologo.TotalPacientes}\n" +
                $"Avaliação: {psicologo.AvaliacaoMedia:F1}★\n\n" +
                $"{psicologo.Biografia}",
                "Fechar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao exibir perfil: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task AgendarConsultaAsync(PsicologoContactoDto psicologo)
    {
        if (psicologo == null) return;

        try
        {
            // Navegar para a página de solicitar sessão
            await Shell.Current.GoToAsync("solicitarsessao", new Dictionary<string, object>
        {
            { "psicologo", psicologo }
        });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir agendamento: {ex.Message}", "OK");
            Console.WriteLine($"❌ Erro ao navegar para solicitarsessao: {ex}");
        }
    }

    /// Inicia um chat com o psicólogo
    [RelayCommand]
    public async Task IniciarChatAsync(PsicologoContactoDto psicologo)
    {
        if (psicologo == null) return;

        try
        {
            var parametros = new Dictionary<string, object>
        {
            { "outroUtilizadorId", psicologo.Id },
            { "outroUtilizadorNome", psicologo.Nome },
            { "outroUtilizadorAvatar", "👨‍⚕️" }
        };

            await Shell.Current.GoToAsync("ChatPage", parametros);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao iniciar chat: {ex.Message}", "OK");
        }
    }


    //[RelayCommand]
    //public async Task EnviarEmailAsync(PsicologoContactoDto psicologo)
    //{
    //    if (psicologo == null) return;

    //    try
    //    {
    //        var message = new EmailMessage
    //        {
    //            Subject = $"Contacto - MindCare+",
    //            Body = $"Olá Dr(a). {psicologo.Nome},\n\nGostaria de contactá-lo através da plataforma MindCare+.\n\n",
    //            To = new List<string> { psicologo.Email }
    //        };

    //        await Email.Default.ComposeAsync(message);
    //    }
    //    catch (Exception ex)
    //    {
    //        await Shell.Current.DisplayAlert(
    //            "Erro",
    //            $"Não foi possível abrir o cliente de email: {ex.Message}",
    //            "OK");
    //    }
    //}
}