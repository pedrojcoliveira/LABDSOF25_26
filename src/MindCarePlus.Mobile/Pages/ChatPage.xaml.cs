using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Subscreve ao evento de nova mensagem para fazer scroll
        _viewModel.Mensagens.CollectionChanged += OnMensagensChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine("📄 ChatPage.OnAppearing - A inicializar chat...");
        await _viewModel.InicializarChatAsync();
    }

    private void OnMensagensChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Scroll para a última mensagem usando CollectionView
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100);

            if (MensagensCollectionView.ItemsSource is System.Collections.IList list && list.Count > 0)
            {
                var ultimaMensagem = list[list.Count - 1];
                MensagensCollectionView.ScrollTo(ultimaMensagem, position: ScrollToPosition.End, animate: false);
            }
        });
    }

    private void OnEnviarMensagem(object sender, EventArgs e)
    {
        _viewModel.EnviarMensagemCommand.Execute(null);
    }
}