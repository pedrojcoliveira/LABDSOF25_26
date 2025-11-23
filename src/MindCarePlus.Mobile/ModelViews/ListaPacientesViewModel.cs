using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs.Paciente;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.ModelViews;

public partial class ListaPacientesViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly ChatService _chatService;

    public ListaPacientesViewModel(ApiService apiService, AuthService authService, ChatService chatService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        Pacientes = new ObservableCollection<PacienteResumoDto>();
    }

    [ObservableProperty]
    private ObservableCollection<PacienteResumoDto> pacientes;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    [ObservableProperty]
    private bool temPacientes;

    [ObservableProperty]
    private string textoPesquisa = string.Empty;

    [ObservableProperty]
    private int totalMensagensNaoLidas;

    private List<PacienteResumoDto> _todosOsPacientes = new();

    /// <summary>
    /// Carrega a lista de pacientes ao inicializar a página
    /// </summary>
    [RelayCommand]
    public async Task CarregarPacientesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        MensagemErro = string.Empty;

        try
        {
            // Verificar autenticação
            if (!await _authService.IsAuthenticatedAsync())
            {
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // Verificar role
            var role = await _authService.GetUserRoleAsync();
            if (role != "Psicologo")
            {
                MensagemErro = "Acesso negado. Apenas psicólogos podem aceder a esta área.";
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            // Carregar pacientes da API
            var pacientes = await _apiService.ListarMeusPacientesAsync();

            _todosOsPacientes = pacientes;
            Pacientes.Clear();

            foreach (var paciente in pacientes)
            {
                Pacientes.Add(paciente);
            }

            TemPacientes = Pacientes.Count > 0;

            if (!TemPacientes)
            {
                MensagemErro = "Ainda não tem pacientes associados.";
            }

            // CARREGAR MENSAGENS NÃO LIDAS
            try
            {
                TotalMensagensNaoLidas = await _chatService.ContarMensagensNaoLidasAsync();
                Console.WriteLine($"📬 {TotalMensagensNaoLidas} mensagens não lidas");
            }
            catch (Exception chatEx)
            {
                Console.WriteLine($"⚠️ Erro ao carregar mensagens: {chatEx.Message}");
                TotalMensagensNaoLidas = 0;
            }
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar pacientes: {ex.Message}";
            Console.WriteLine($"Erro em CarregarPacientesAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Recarrega a lista com pull-to-refresh
    /// </summary>
    [RelayCommand]
    public async Task RefrescarPacientesAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;

        try
        {
            await CarregarPacientesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Navega para os detalhes de um paciente selecionado
    /// </summary>
    [RelayCommand]
    public async Task SelecionarPacienteAsync(PacienteResumoDto paciente)
    {
        if (paciente == null) return;

        try
        {
            // Navegar para a página de detalhes passando o ID do paciente
            await Shell.Current.GoToAsync($"DetalhePacientePage?pacienteId={paciente.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro",
                $"Erro ao abrir detalhes do paciente: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Pesquisa pacientes por nome
    /// </summary>
    [RelayCommand]
    public void PesquisarPacientes()
    {
        if (string.IsNullOrWhiteSpace(TextoPesquisa))
        {
            // Mostrar todos os pacientes
            Pacientes.Clear();
            foreach (var paciente in _todosOsPacientes)
            {
                Pacientes.Add(paciente);
            }
        }
        else
        {
            // Filtrar por nome (case-insensitive)
            var filtrados = _todosOsPacientes
                .Where(p => p.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Pacientes.Clear();
            foreach (var paciente in filtrados)
            {
                Pacientes.Add(paciente);
            }
        }

        TemPacientes = Pacientes.Count > 0;
    }

    /// <summary>
    /// Limpa o campo de pesquisa
    /// </summary>
    [RelayCommand]
    public void LimparPesquisa()
    {
        TextoPesquisa = string.Empty;
        PesquisarPacientes();
    }

    [RelayCommand]
    private async Task AbrirMensagens()
    {
        try
        {
            await Shell.Current.GoToAsync("//ConversasPage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao navegar para ConversasPage: {ex.Message}");
            await Shell.Current.DisplayAlert("Erro",
                $"Erro ao abrir mensagens: {ex.Message}", "OK");
        }
    }
}