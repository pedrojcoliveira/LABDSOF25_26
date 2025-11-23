using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile.Pages;

public partial class ConversasPage : ContentPage
{
    private readonly ConversasViewModel _viewModel;

    public ConversasPage(ConversasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Console.WriteLine("📄 ConversasPage.OnAppearing - A carregar conversas...");
        await _viewModel.CarregarConversasAsync();
    }
}