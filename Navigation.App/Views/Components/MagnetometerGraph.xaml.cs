namespace Navigation.App.Views.Components;

public partial class MagnetometerGraph : ContentView
{

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(MagnetometerGraph));

    public MagnetometerGraph()
    {
        InitializeComponent();
    }
}