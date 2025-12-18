using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

[QueryProperty(nameof(OutroUtilizadorId), "outroUtilizadorId")]
[QueryProperty(nameof(OutroUtilizadorNome), "outroUtilizadorNome")]
[QueryProperty(nameof(OutroUtilizadorAvatar), "outroUtilizadorAvatar")]
public partial class ChatViewModel : ObservableObject
{
    private readonly ChatService _chatService;

    // ✅ Propriedade observável (gera automaticamente MeuId público)
    [ObservableProperty]
    private Guid meuId;

    [ObservableProperty]
    private Guid outroUtilizadorId;

    [ObservableProperty]
    private string outroUtilizadorNome = string.Empty;

    [ObservableProperty]
    private string outroUtilizadorAvatar = "👤";

    [ObservableProperty]
    private ObservableCollection<MensagemDto> mensagens = new();

    [ObservableProperty]
    private string novaMensagem = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    public ChatViewModel(ChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        _chatService.MensagemRecebida += OnMensagemRecebida;
        _chatService.MensagemEnviada += OnMensagemEnviada;
        _chatService.ConexaoAlterada += OnConexaoAlterada;
    }

    [RelayCommand]
    public async Task InicializarChatAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("🔍 ChatViewModel.InicializarChatAsync");

        IsLoading = true;

        try
        {
            // Obtém ID do utilizador atual
            var userId = await SecureStorage.GetAsync("user_id");
            Console.WriteLine($"📝 user_id do SecureStorage: '{userId}'");

            if (!string.IsNullOrEmpty(userId))
            {
                MeuId = Guid.Parse(userId); // ✅ Usa MeuId (maiúscula)
                Console.WriteLine($"✅ MeuId: {MeuId}");
            }
            else
            {
                Console.WriteLine("❌ user_id não encontrado no SecureStorage!");
            }

            // Liga ao SignalR
            if (!_chatService.IsConnected)
            {
                Console.WriteLine("🔌 A ligar ao SignalR...");
                await _chatService.LigarAsync();
            }

            IsConnected = _chatService.IsConnected;
            Console.WriteLine($"✅ IsConnected: {IsConnected}");

            // Carrega histórico
            Console.WriteLine("📖 A carregar histórico...");
            await CarregarHistoricoAsync();

            Console.WriteLine("✅ Inicialização concluída");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }

        Console.WriteLine("========================================");
    }

    [RelayCommand]
    public async Task CarregarHistoricoAsync()
    {
        try
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"🔍 CarregarHistoricoAsync");
            Console.WriteLine($"   MeuId: {MeuId}");
            Console.WriteLine($"   OutroUtilizadorId: {OutroUtilizadorId}");

            var historico = await _chatService.ObterHistoricoAsync(OutroUtilizadorId);

            Console.WriteLine($"✅ Histórico recebido: {historico?.Count ?? 0} mensagens");

            Mensagens.Clear();

            if (historico != null && historico.Any())
            {
                foreach (var mensagem in historico)
                {
                    Console.WriteLine($"📩 Mensagem:");
                    Console.WriteLine($"   RemetenteId: {mensagem.RemetenteId}");
                    Console.WriteLine($"   RemetenteNome: {mensagem.RemetenteNome}");
                    Console.WriteLine($"   É minha? {mensagem.RemetenteId == MeuId}");

                    Mensagens.Add(mensagem);
                }
            }

            Console.WriteLine($"✅ Total: {Mensagens.Count} mensagens");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task EnviarMensagemAsync()
    {
        if (string.IsNullOrWhiteSpace(NovaMensagem)) return;
        if (!IsConnected) return;

        var mensagemTexto = NovaMensagem.Trim();
        NovaMensagem = string.Empty;

        try
        {
            var mensagemDto = new EnviarMensagemDto
            {
                DestinatarioId = OutroUtilizadorId,
                Conteudo = mensagemTexto
            };

            await _chatService.EnviarMensagemAsync(mensagemDto);
            Console.WriteLine($"📤 Mensagem enviada: {mensagemTexto}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro",
                $"Erro ao enviar mensagem: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task FecharChatAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnMensagemRecebida(MensagemDto mensagem)
    {
        if (mensagem.RemetenteId == OutroUtilizadorId)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Mensagens.Add(mensagem);
                Console.WriteLine($"💬 Nova mensagem recebida");
            });
        }
    }

    private void OnMensagemEnviada(MensagemDto mensagem)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Console.WriteLine("========================================");
            Console.WriteLine("🔍 OnMensagemEnviada - CONFIRMAÇÃO DO SIGNALR");
            Console.WriteLine($"   Conteudo: {mensagem.Conteudo}");
            Console.WriteLine($"   EhMinhaMensagem (ANTES): {mensagem.EhMinhaMensagem}");

            // FORÇA AQUI!
            mensagem.EhMinhaMensagem = true; // ROXO À DIREITA! - Mensagem enviada

            Console.WriteLine($"   EhMinhaMensagem (DEPOIS): {mensagem.EhMinhaMensagem}");
            Console.WriteLine("   🟣 DEVE APARECER ROXO À DIREITA!");
            Console.WriteLine("========================================");

            Mensagens.Add(mensagem);
        });
    }

    private void OnConexaoAlterada(bool ligado)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsConnected = ligado;
        });
    }

    /// <summary>
    /// Abre o perfil do paciente com quem está a conversar
    /// UM.PS.05 - Navegação a partir do chat ativo
    /// </summary>
    [RelayCommand]
    public async Task AbrirPerfilAsync()
    {
        try
        {
            await Shell.Current.GoToAsync($"DetalhePacientePage?pacienteId={OutroUtilizadorId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao abrir perfil: {ex.Message}");
            await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir perfil: {ex.Message}", "OK");
        }
    }
}