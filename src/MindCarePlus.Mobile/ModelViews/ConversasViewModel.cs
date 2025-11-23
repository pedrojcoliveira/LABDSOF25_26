using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindCarePlus.Mobile.Services;
using MindCarePlus.Shared.DTOs;
using System.Collections.ObjectModel;

namespace MindCarePlus.Mobile.ModelViews;

public partial class ConversasViewModel : ObservableObject
{
    private readonly ChatService _chatService;

    [ObservableProperty]
    private ObservableCollection<ConversaDto> conversas = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool temConversas;

    [ObservableProperty]
    private string mensagemErro = string.Empty;

    public ConversasViewModel(ChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        // Subscreve eventos
        _chatService.MensagemRecebida += OnMensagemRecebida;
    }

    [RelayCommand]
    public async Task CarregarConversasAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        MensagemErro = string.Empty;

        try
        {
            // Liga ao chat se ainda não estiver
            if (!_chatService.IsConnected)
            {
                await _chatService.LigarAsync();
            }

            var conversasLista = await _chatService.ListarConversasAsync();

            Conversas.Clear();
            foreach (var conversa in conversasLista)
            {
                Conversas.Add(conversa);
            }

            TemConversas = Conversas.Count > 0;

            Console.WriteLine($"✅ {Conversas.Count} conversas carregadas");
        }
        catch (Exception ex)
        {
            MensagemErro = $"Erro ao carregar conversas: {ex.Message}";
            Console.WriteLine($"❌ {MensagemErro}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RefrescarConversasAsync()
    {
        if (IsRefreshing) return;

        IsRefreshing = true;

        try
        {
            await CarregarConversasAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task AbrirChatAsync(ConversaDto conversa)
    {
        if (conversa == null) return;

        try
        {
            var parametros = new Dictionary<string, object>
            {
                { "outroUtilizadorId", conversa.OutroUtilizadorId },
                { "outroUtilizadorNome", conversa.OutroUtilizadorNome },
                { "outroUtilizadorAvatar", conversa.OutroUtilizadorAvatar }
            };

            await Shell.Current.GoToAsync("ChatPage", parametros);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir chat: {ex.Message}", "OK");
        }
    }

    private void OnMensagemRecebida(MensagemDto mensagem)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Console.WriteLine($"📬 Nova mensagem recebida - atualizando lista");
            await CarregarConversasAsync();
        });
    }
}