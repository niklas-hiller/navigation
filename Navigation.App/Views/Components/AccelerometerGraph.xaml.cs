namespace Navigation.App.Views.Components;

public partial class AccelerometerGraph : ContentView
{

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AccelerometerGraph));

    public AccelerometerGraph()
    {
        InitializeComponent();
    }
}