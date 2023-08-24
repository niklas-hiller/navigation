using CommunityToolkit.Mvvm.ComponentModel;
using Navigation.App.Services;

namespace Navigation.App.ViewModels.Sensors;

public partial class PositionViewModel : ObservableObject
{
    [ObservableProperty]
    public Location initialPosition;

    [ObservableProperty]
    public Location currentPosition;

    [ObservableProperty]
    public double distance;

    public PositionViewModel()
    {
        var service = Services.ServiceProvider.GetService<SensorService>();
        service.Geolocations.CollectionChanged += Geolocations_CollectionChanged;
    }

    private void Geolocations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Location location = (Location)e.NewItems[0];
        if (InitialPosition == null)
        {
            InitialPosition = location;
        }
        CurrentPosition = location;

        Distance = InitialPosition.CalculateDistance(CurrentPosition, DistanceUnits.Kilometers) * 1000;
    }
}
