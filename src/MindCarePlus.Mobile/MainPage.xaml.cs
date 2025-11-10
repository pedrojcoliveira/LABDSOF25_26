using MindCarePlus.Mobile.ModelViews;

namespace MindCarePlus.Mobile;

public partial class MainPage : ContentPage
{
	private readonly MainPageModelView _modelView;



    public MainPage(MainPageModelView modelView)
    {
        InitializeComponent();
        _modelView = modelView;
        BindingContext = _modelView;
    }

}
