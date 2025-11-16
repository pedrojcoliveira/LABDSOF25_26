using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;
namespace MindCarePlus.Mobile.ModelViews
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        public DashboardViewModel(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
            BadgesConquistadas = new ObservableCollection<BadgeConquistadaDto>();
            EvolucaoHumor = new ObservableCollection<DiaHumorDto>();
        }

        [ObservableProperty]
        private string nomeUtilizador = string.Empty;

        [ObservableProperty]
        private int nivelAtual;

        [ObservableProperty]
        private int pontosAtuais;

        [ObservableProperty]
        private int pontosParaProximoNivel;

        [ObservableProperty]
        private double progressoPercentagem;

        [ObservableProperty]
        private int sequenciaDias;

        [ObservableProperty]
        private int totalRegistos;

        [ObservableProperty]
        private int totalBadges;

        [ObservableProperty]
        private ObservableCollection<BadgeConquistadaDto> badgesConquistadas; 

        [ObservableProperty]
        private ObservableCollection<DiaHumorDto> evolucaoHumor;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string mensagemErro = string.Empty;

        public async Task LoadDashboardAsync()
        {
            IsLoading = true;
            MensagemErro = string.Empty;

            try
            {
                // 1. Verifica se está autenticado
                if (!await _authService.IsAuthenticatedAsync())
                {
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // 2. Verifica se é Paciente
                var role = await _authService.GetUserRoleAsync();
                if (role != "Paciente")
                {
                    MensagemErro = "Acesso negado. Apenas pacientes podem aceder à dashboard.";
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                // 3. Obtém o ID do paciente do token JWT
                var pacienteId = await _authService.GetCurrentUserIdAsync();
                if (pacienteId == null)
                {
                    MensagemErro = "Erro ao obter dados do utilizador.";
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                Console.WriteLine($"PacienteId obtido do token: {pacienteId}");

                // 4. Busca os dados da dashboard
                var response = await _apiService.GetDashboardDataAsync(pacienteId.Value);

                if (response == null)
                {
                    MensagemErro = "Erro ao carregar dados da dashboard.";
                    return;
                }

                // 5. Preenche os dados
                NomeUtilizador = response.NomeUtilizador;
                NivelAtual = response.NivelAtual;
                PontosAtuais = response.PontosAtuais;
                PontosParaProximoNivel = response.PontosParaProximoNivel;
                ProgressoPercentagem = response.ProgressoPercentagem;
                SequenciaDias = response.SequenciaDias;
                TotalRegistos = response.TotalRegistos;
                TotalBadges = response.TotalBadges;

                BadgesConquistadas.Clear();
                foreach (var badge in response.BadgesConquistadas)
                {
                    BadgesConquistadas.Add(badge);
                }

                EvolucaoHumor.Clear();
                foreach (var dia in response.EvolucaoHumor)
                {
                    EvolucaoHumor.Add(dia);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro : {ex.Message}");
                MensagemErro = $"Erro : {ex.Message}";
            }
 
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RegistarHumor()
        {
            await Shell.Current.GoToAsync("RegistoHumor");
        }

        [RelayCommand]
        private async Task VerGraficoEvolucao()
        {
            await Shell.Current.GoToAsync("evolucao");
        }

        [RelayCommand]
        private async Task VerDesafios()
        {
            await Shell.Current.GoToAsync("DesafiosPage");
        }

        [RelayCommand]
        private async Task ContactarPsicologo()
        {
            await Shell.Current.GoToAsync("contactar");
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}