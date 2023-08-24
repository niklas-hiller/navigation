using System.Globalization;
using System.Numerics;

namespace Navigation.Neural
{
    public readonly record struct DataRow
    {
        public readonly Vector3 Accelerometer;
        public readonly Vector3 Gyroscope;
        public readonly Vector3 Magnetometer;
        public readonly Vector3 LastMeasurement;
        public static readonly int SequenceLength = 4;
        public static readonly int InputSize = 3;

        public readonly Vector2 Position;
        public static readonly int OutputSize = 2;

        private static NumberFormatInfo nfi = new NumberFormatInfo();

        public DataRow(Vector3 accelerometer, Vector3 gyroscope, Vector3 magnetometer, Vector3 lastMeasurement, Vector2 position)
        {
            Accelerometer = accelerometer;
            Gyroscope = gyroscope;
            Magnetometer = magnetometer;
            LastMeasurement = lastMeasurement;
            Position = position;
        }

        public Vector3 GetInputVector(int num)
        {
            switch (num)
            {
                case 0:
                    return Accelerometer;
                case 1:
                    return Gyroscope;
                case 2:
                    return Magnetometer;
                case 3:
                    return LastMeasurement;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void Export(List<DataRow> dataRows, string filePath)
        {
            nfi.NumberDecimalSeparator = ".";

            using (var writer = new StreamWriter(filePath))
            {
                // Schreibe die Spaltenüberschriften
                writer.WriteLine("AccelerometerX,AccelerometerY,AccelerometerZ,GyroscopeX,GyroscopeY,GyroscopeZ,MagnetometerX,MagnetometerY,MagnetometerZ,LastMeasurementX,LastMeasurementY,LastMeasurementZ,PositionX,PositionY");

                // Schreibe die Datenzeilen
                foreach (var row in dataRows)
                {
                    writer.Write(row.Accelerometer.X.ToString(nfi) + ",");
                    writer.Write(row.Accelerometer.Y.ToString(nfi) + ",");
                    writer.Write(row.Accelerometer.Z.ToString(nfi) + ",");
                    writer.Write(row.Gyroscope.X.ToString(nfi) + ",");
                    writer.Write(row.Gyroscope.Y.ToString(nfi) + ",");
                    writer.Write(row.Gyroscope.Z.ToString(nfi) + ",");
                    writer.Write(row.Magnetometer.X.ToString(nfi) + ",");
                    writer.Write(row.Magnetometer.Y.ToString(nfi) + ",");
                    writer.Write(row.Magnetometer.Z.ToString(nfi) + ",");
                    writer.Write(row.LastMeasurement.X + ",");
                    writer.Write(row.LastMeasurement.Y + ",");
                    writer.Write(row.LastMeasurement.Z + ",");
                    writer.Write(row.Position.X.ToString(nfi) + ",");
                    writer.WriteLine(row.Position.Y.ToString(nfi));
                }
            }
        }

        public static float ParseFloat(string s)
        {
            nfi.NumberDecimalSeparator = ".";
            return float.Parse(s, nfi);
        }
    }
}
