namespace Navigation.App.Views;

public partial class DevicePage : ContentPage
{
    private async void OnButton_Clicked1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AuthorizedDevice());
    }

    private async void OnButton_Clicked2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisteredDevice());
    }

    public DevicePage()
	{
		InitializeComponent();
	}
}