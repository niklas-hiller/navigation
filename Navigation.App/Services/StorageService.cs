using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Navigation.App.Services
{
    public class StorageService : IStorageService
    {
        private const string storageFileName = "sensor_measurements.csv";
        private List<string> data = new List<string>();
        private NumberFormatInfo nfi = new NumberFormatInfo();

        IFileSaver fileSaver;

        public StorageService(IFileSaver fileSaver)
        {
            this.fileSaver = fileSaver;
            nfi.NumberDecimalSeparator = ".";

        }

        public async Task exportSensorData()
        {
            string headerLine =
                "AccelerometerX,AccelerometerY,AccelerometerZ," +
                "MagnetometerX,MagnetometerY,MagnetometerZ," +
                "GyroscopeX,GyroscopeY,GyroscopeZ," +
                "GeolocationLatitude,GeolocationLongitude,GeolocationAccuracy," +
                "Datetime\n";
            string dataString = headerLine + String.Join("\n", data.ToArray());

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            using var stream = new MemoryStream(Encoding.Default.GetBytes(dataString));
            var fileSaverResult = await fileSaver.SaveAsync(storageFileName, stream, token);
            if (fileSaverResult.IsSuccessful)
            {
                await Toast.Make($"File is saved: {fileSaverResult.FilePath}").Show(token);
            } 
            else
            {
                await Toast.Make($"Export cancelled.").Show(token);
            }
        }

        private void saveSensorData(DateTimeOffset timestamp, Vector3? accelerometerData = null, Vector3? magnetometerData = null,
            Vector3? gyroscopeData = null, Vector3? geolocationData = null)
        {
            string accelerometerMeasurement = ",,";
            string magnetometerMeasurement = ",,";
            string gyroscopeMeasurement = ",,";
            string geolocationMeasurement = ",,";
            if (accelerometerData.HasValue)
            {
                accelerometerMeasurement =
                    $"{accelerometerData.Value.X.ToString(nfi)}," +
                    $"{accelerometerData.Value.Y.ToString(nfi)}," +
                    $"{accelerometerData.Value.Z.ToString(nfi)}";
            }
            if (magnetometerData.HasValue)
            {
                magnetometerMeasurement =
                    $"{magnetometerData.Value.X.ToString(nfi)}," +
                    $"{magnetometerData.Value.Y.ToString(nfi)}," +
                    $"{magnetometerData.Value.Z.ToString(nfi)}";
            }
            if (gyroscopeData.HasValue)
            {
                gyroscopeMeasurement =
                    $"{gyroscopeData.Value.X.ToString(nfi)}," +
                    $"{gyroscopeData.Value.Y.ToString(nfi)}," +
                    $"{gyroscopeData.Value.Z.ToString(nfi)}";
            }
            if (geolocationData.HasValue)
            {
                geolocationMeasurement =
                    $"{geolocationData.Value.X.ToString(nfi)}," +
                    $"{geolocationData.Value.Y.ToString(nfi)}," +
                    $"{geolocationData.Value.Z.ToString(nfi)}";
            }

            string csvLine =
                $"{accelerometerMeasurement}," +
                $"{magnetometerMeasurement}," +
                $"{gyroscopeMeasurement}," +
                $"{geolocationMeasurement}," +
                $"{timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")}";

            data.Add(csvLine);
        }

        public void saveMagnetometer(Vector3 vector3)
        {
            saveSensorData(DateTimeOffset.Now, magnetometerData: vector3);
        }

        public void saveGyroscope(Vector3 vector3)
        {
            saveSensorData(DateTimeOffset.Now, gyroscopeData: vector3);
        }

        public void saveAccelerometer(Vector3 vector3)
        {
            saveSensorData(DateTimeOffset.Now, accelerometerData: vector3);
        }

        public void saveGeolocation(Location location)
        {
            Vector3 vector3 = new Vector3((float)location.Latitude, (float)location.Longitude, (float)location.Accuracy);
            saveSensorData(DateTimeOffset.Now, geolocationData: vector3);
        }
    }
}
