using System.Numerics;

namespace Navigation.App.Services
{
    public interface IStorageService
    {
        Task exportSensorData();
        void saveMagnetometer(Vector3 vector3);
        void saveGyroscope(Vector3 vector3);
        void saveAccelerometer(Vector3 vector3);
        void saveGeolocation(Location location);
    }
}
