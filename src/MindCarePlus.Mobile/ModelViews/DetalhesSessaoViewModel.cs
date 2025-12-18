using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using MindCarePlus.Shared.DTOs.Paciente;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

// Removemos o [QueryProperty] e usamos a interface IQueryAttributable, que é mais segura
public partial class DetalhesSessaoViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;

    public DetalhesSessaoViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private SessaoCalendarioDto sessao;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    // Resumo do Paciente
    [ObservableProperty]
    private ObservableCollection<RegistoHumorDto> ultimosRegistos = new();

    [ObservableProperty]
    private RelatorioIAResumoDto ultimoRelatorio;

    [ObservableProperty]
    private bool temRelatorios;

    [ObservableProperty]
    private bool temRegistos;

    // Este método é chamado automaticamente pelo Shell quando a navegação ocorre
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Console.WriteLine("📥 DetalhesSessaoViewModel: A receber parâmetros de navegação...");

        if (query.ContainsKey("Sessao"))
        {
            var dados = query["Sessao"] as SessaoCalendarioDto;
            if (dados != null)
            {
                Console.WriteLine($"✅ Sessão recebida: {dados.PacienteNome} - {dados.DataFormatada}");
                Sessao = dados;

                // Dispara o carregamento dos dados extra (sem await para não bloquear a UI)
                _ = CarregarContextoPacienteAsync(Sessao.PacienteId);
            }
            else
            {
                Console.WriteLine("❌ O objeto 'Sessao' é nulo ou do tipo errado.");
            }
        }
        else
        {
            Console.WriteLine("❌ A chave 'Sessao' não foi encontrada nos parâmetros.");
        }
    }

    private async Task CarregarContextoPacienteAsync(Guid pacienteId)
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            Console.WriteLine($"🔄 A carregar detalhes do paciente {pacienteId}...");

            // Verifica se apiService não é null
            if (_apiService == null)
            {
                Console.WriteLine("❌ ApiService é null!");
                return;
            }

            var detalhes = await _apiService.ObterDetalhesPacienteAsync(pacienteId);

            if (detalhes != null)
            {
                // Carregar últimos registos
                UltimosRegistos.Clear();
                if (detalhes.HistoricoHumor != null)
                {
                    foreach (var registo in detalhes.HistoricoHumor.Take(3))
                    {
                        UltimosRegistos.Add(registo);
                    }
                }
                TemRegistos = UltimosRegistos.Any();

                // Carregar relatório
                if (detalhes.RelatoriosIA != null && detalhes.RelatoriosIA.Any())
                {
                    UltimoRelatorio = detalhes.RelatoriosIA.OrderByDescending(r => r.DataGeracao).First();
                    TemRelatorios = true;
                }
                else
                {
                    TemRelatorios = false;
                }

                Console.WriteLine("✅ Dados extra do paciente carregados.");
            }
        }
        catch (Exception ex)
        {
            MensagemErro = "Não foi possível carregar o histórico.";
            Console.WriteLine($"❌ Erro ao carregar contexto: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task VoltarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task IrParaPerfilCompletoAsync()
    {
        if (Sessao != null)
        {
            await Shell.Current.GoToAsync($"DetalhePacientePage?pacienteId={Sessao.PacienteId}");
        }
    }

    [RelayCommand]
    public async Task VerRelatorioCompletoAsync()
    {
        if (UltimoRelatorio == null || Sessao == null) return;

        // Navega para o perfil, passando o ID e o parâmetro 'tab'
        // Nota: Usamos query string (?param=valor&param2=valor)
        await Shell.Current.GoToAsync($"DetalhePacientePage?pacienteId={Sessao.PacienteId}&tab=relatorios");
    }
}