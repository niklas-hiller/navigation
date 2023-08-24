using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;

namespace Navigation.App.Services
{
    public class SensorService : ObservableObject
    {
        private readonly IStorageService _storageService;

        private static readonly int UPPER_LIMIT = 50;

        public List<Vector3> AccelerometerData = new List<Vector3>();
        public ObservableCollection<ObservableValue> AccelerometerX = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> AccelerometerY = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> AccelerometerZ = new ObservableCollection<ObservableValue>();

        public List<Vector3> GyroscopeData = new List<Vector3>();
        public ObservableCollection<ObservableValue> GyroscopeRoll = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> GyroscopePitch = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> GyroscopeYaw = new ObservableCollection<ObservableValue>();

        public List<Vector3> MagnetometerData = new List<Vector3>();
        public ObservableCollection<ObservableValue> MagnetometerX = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> MagnetometerY = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> MagnetometerZ = new ObservableCollection<ObservableValue>();

        public ObservableCollection<Location> Geolocations = new ObservableCollection<Location>();

        public SensorService(IStorageService storageService)
        {
            _storageService = storageService;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;

        }

        public async Task TrackGeolocation(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                stopwatch.Restart();
                GeolocationRequest geolocationRequest = new GeolocationRequest(GeolocationAccuracy.Best);
                Location loc = await Geolocation.GetLocationAsync(geolocationRequest, cancellationToken);
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"Received new location in {stopwatch.Elapsed.TotalSeconds} seconds.");
                Geolocations.Add(loc);
                System.Diagnostics.Debug.WriteLine($"Longitude: {loc.Longitude}; Latitude: {loc.Latitude}; Accuracy: {loc.Accuracy}m;");

                _storageService.saveGeolocation(loc);
            }
        }

        public void Start()
        {
            // Überprüfen, ob der Accelerometer-Sensor verfügbar ist
            if (Accelerometer.IsMonitoring)
                Accelerometer.Stop();

            // Beginnen der Überwachung des Accelerometers mit einer gewünschten Aktualisierungsrate
            Accelerometer.Start(SensorSpeed.UI);

            // Überprüfen, ob der Gyroscope-Sensor verfügbar ist
            if (Gyroscope.IsMonitoring)
                Gyroscope.Stop();

            // Beginnen der Überwachung des Gyroscopes mit einer gewünschten Aktualisierungsrate
            Gyroscope.Start(SensorSpeed.UI);

            // Überprüfen, ob der Magnetometer-Sensor verfügbar ist
            if (Magnetometer.IsMonitoring)
                Magnetometer.Stop();

            // Beginnen der Überwachung des Magnetometers mit einer gewünschten Aktualisierungsrate
            Magnetometer.Start(SensorSpeed.UI);
        }

        public void Stop()
        {
            if (Accelerometer.IsMonitoring)
                Accelerometer.Stop();
            if (Gyroscope.IsMonitoring)
                Gyroscope.Stop();
            if (Magnetometer.IsMonitoring)
                Magnetometer.Stop();
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var Acceleration = e.Reading.Acceleration;
            AccelerometerData.Add(Acceleration);

            // System.Diagnostics.Debug.WriteLine($"Acceleration: {AccelerometerData.Count}.");

            // Aktualisieren der Werte im ViewModel, wenn sich der Sensorwert ändert
            if (AccelerometerX.Count > UPPER_LIMIT)
            {
                AccelerometerX.RemoveAt(0);
            }
            if (AccelerometerY.Count > UPPER_LIMIT)
            {
                AccelerometerY.RemoveAt(0);
            }
            if (AccelerometerZ.Count > UPPER_LIMIT)
            {
                AccelerometerZ.RemoveAt(0);
            }

            AccelerometerX.Add(new(Acceleration.X));
            AccelerometerY.Add(new(Acceleration.Y));
            AccelerometerZ.Add(new(Acceleration.Z));

            _storageService.saveAccelerometer(Acceleration);
        }

        private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            var AngularVelocity = e.Reading.AngularVelocity;
            GyroscopeData.Add(AngularVelocity);

            // System.Diagnostics.Debug.WriteLine($"Gyroscope: {GyroscopeData.Count}.");

            // Aktualisieren der Werte im ViewModel, wenn sich der Sensorwert ändert
            if (GyroscopeRoll.Count > UPPER_LIMIT)
            {
                GyroscopeRoll.RemoveAt(0);
            }
            if (GyroscopePitch.Count > UPPER_LIMIT)
            {
                GyroscopePitch.RemoveAt(0);
            }
            if (GyroscopeYaw.Count > UPPER_LIMIT)
            {
                GyroscopeYaw.RemoveAt(0);
            }
            GyroscopeRoll.Add(new(AngularVelocity.X));
            GyroscopePitch.Add(new(AngularVelocity.Y));
            GyroscopeYaw.Add(new(AngularVelocity.Z));

            _storageService.saveGyroscope(AngularVelocity);
        }

        private void Magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e)
        {
            var MagneticField = e.Reading.MagneticField;
            MagnetometerData.Add(MagneticField);

            // System.Diagnostics.Debug.WriteLine($"Magnetometer: {MagnetometerData.Count}.");

            // Aktualisieren der Werte im ViewModel, wenn sich der Sensorwert ändert
            if (MagnetometerX.Count > UPPER_LIMIT)
            {
                MagnetometerX.RemoveAt(0);
            }
            if (MagnetometerY.Count > UPPER_LIMIT)
            {
                MagnetometerY.RemoveAt(0);
            }
            if (MagnetometerZ.Count > UPPER_LIMIT)
            {
                MagnetometerZ.RemoveAt(0);
            }
            MagnetometerX.Add(new(MagneticField.X));
            MagnetometerY.Add(new(MagneticField.Y));
            MagnetometerZ.Add(new(MagneticField.Z));

            _storageService.saveMagnetometer(MagneticField);
        }
    }
}
