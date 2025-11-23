using Microsoft.AspNetCore.SignalR.Client;
using MindCarePlus.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MindCarePlus.Mobile.Services;

public class ChatService : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private bool _isConnected;

    // EVENTOS
    public event Action<MensagemDto>? MensagemRecebida;
    public event Action<MensagemDto>? MensagemEnviada;
    public event Action<bool>? ConexaoAlterada;

    public bool IsConnected => _isConnected;

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task ConfigureAuthHeaderAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task LigarAsync()
    {
        if (_isConnected) return;

        try
        {
            var token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Token não encontrado. Não é possível ligar ao chat.");
                return;
            }

            var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/chathub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token)!;

#if ANDROID
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, cert, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };
#endif
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                .Build();

            // HANDLERS DE EVENTOS SIGNALR

            // Receber mensagem
            _hubConnection.On<MensagemDto>("ReceberMensagem", (mensagem) =>
            {
                Console.WriteLine($"💬 Mensagem recebida: {mensagem.RemetenteNome} → {mensagem.Conteudo}");
                MensagemRecebida?.Invoke(mensagem);
            });

            // Confirmação de mensagem enviada
            _hubConnection.On<MensagemDto>("MensagemEnviada", (mensagem) =>
            {
                Console.WriteLine($"✅ Mensagem enviada confirmada: {mensagem.Conteudo}");
                MensagemEnviada?.Invoke(mensagem);
            });

            // Erro ao enviar
            _hubConnection.On<string>("ErroEnvio", (erro) =>
            {
                Console.WriteLine($"❌ Erro no envio: {erro}");
            });

            // EVENTOS DE CONEXÃO
            _hubConnection.Reconnecting += (exception) =>
            {
                Console.WriteLine("⚠️ A religar ao chat...");
                _isConnected = false;
                ConexaoAlterada?.Invoke(false);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"✅ Religado ao chat! ConnectionId: {connectionId}");
                _isConnected = true;
                ConexaoAlterada?.Invoke(true);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += (exception) =>
            {
                Console.WriteLine($"❌ Ligação fechada: {exception?.Message}");
                _isConnected = false;
                ConexaoAlterada?.Invoke(false);
                return Task.CompletedTask;
            };

            // Inicia ligação
            await _hubConnection.StartAsync();
            _isConnected = true;
            ConexaoAlterada?.Invoke(true);

            Console.WriteLine($"✅ Ligado ao chat! ConnectionId: {_hubConnection.ConnectionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao ligar ao chat: {ex.Message}");
            _isConnected = false;
            ConexaoAlterada?.Invoke(false);
            throw;
        }
    }

    public async Task DesligarAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            _isConnected = false;
            ConexaoAlterada?.Invoke(false);
            Console.WriteLine("🔌 Desligado do chat");
        }
    }

    public async Task EnviarMensagemAsync(EnviarMensagemDto mensagem)
    {
        if (!_isConnected || _hubConnection == null)
        {
            throw new InvalidOperationException("Não está ligado ao chat");
        }

        try
        {
            await _hubConnection.InvokeAsync("EnviarMensagem", mensagem);
            Console.WriteLine($"📤 A enviar mensagem para {mensagem.DestinatarioId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao enviar mensagem: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ConversaDto>> ListarConversasAsync()
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            Console.WriteLine("🔍 A procurar conversas...");

            var conversas = await _httpClient.GetFromJsonAsync<List<ConversaDto>>("api/Mensagens/conversas");

            Console.WriteLine($"✅ {conversas?.Count ?? 0} conversas encontradas");

            return conversas ?? new List<ConversaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao listar conversas: {ex.Message}");
            return new List<ConversaDto>();
        }
    }

    public async Task<List<MensagemDto>> ObterHistoricoAsync(Guid outroUtilizadorId)
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            Console.WriteLine($"🔍 A procurar histórico com {outroUtilizadorId}");

            var mensagens = await _httpClient.GetFromJsonAsync<List<MensagemDto>>(
                $"api/Mensagens/historico/{outroUtilizadorId}");

            Console.WriteLine($"✅ {mensagens?.Count ?? 0} mensagens no histórico");

            return mensagens ?? new List<MensagemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao obter histórico: {ex.Message}");
            return new List<MensagemDto>();
        }
    }

    public async Task<int> ContarMensagensNaoLidasAsync()
    {
        await ConfigureAuthHeaderAsync();

        try
        {
            var total = await _httpClient.GetFromJsonAsync<int>("api/Mensagens/nao-lidas/total");

            Console.WriteLine($"📬 {total} mensagens não lidas");

            return total;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao contar mensagens: {ex.Message}");
            return 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}