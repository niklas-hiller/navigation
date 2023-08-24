using Navigation.App.Services;

namespace Navigation.App.Views;

public partial class InsightPage : ContentPage
{
    private readonly IStorageService _storageService;

    public InsightPage(IStorageService storageService)
    {
        InitializeComponent();

        _storageService = storageService;
    }

    private async void Button_OnClicked(object sender, EventArgs e)
    {
        await _storageService.exportSensorData();
    }
}