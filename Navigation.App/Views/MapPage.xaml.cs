using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Navigation.App.Views;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();

        //var mapView = new MapView();

        //mapView.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
        //mapView.MyLocationLayer.UpdateMyLocation(new Position(52.374669, 9.737723), true);
        //mapView.MyLocationFollow = true;
        //mapView.IsZoomButtonVisible = false;
        //mapView.IsMyLocationButtonVisible = false;
        //mapView.IsNorthingButtonVisible = false;
        //mapView.PanLock = true;
        //mapView.ZoomLock = true;
        //Content = mapView;
    }
}