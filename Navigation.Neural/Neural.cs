using OneOf.Types;
using System.Numerics;
using Tensorflow.Keras;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace Navigation.Neural
{
    public class Neural
    {
        int epochs = 10;

        public void Run()
        {
            Model model = BuildModel();

            var stand_data = LoadData("sensor_measurements (stillstehend).csv", 0.3f, nullPos: true);
            var walk_data = LoadData("sensor_measurements (gehen).csv", 0.3f);
            var train_data = LoadData("sensor_measurements (zug).csv", 0.3f);
            var bike_data = LoadData("sensor_measurements (fahrrad).csv", 0.3f);

            var stand_data_prepared_train = PrepareData(stand_data.Item1);
            var walk_data_prepared_train = PrepareData(walk_data.Item1);
            var train_data_prepared_train = PrepareData(train_data.Item1);
            var bike_data_prepared_train = PrepareData(bike_data.Item1);

            var stand_data_prepared_test = PrepareData(stand_data.Item2);
            var walk_data_prepared_test = PrepareData(walk_data.Item2);
            var train_data_prepared_test = PrepareData(train_data.Item2);
            var bike_data_prepared_test = PrepareData(bike_data.Item2);

            Train(model, stand_data_prepared_train.Item1, stand_data_prepared_train.Item2);
            Train(model, walk_data_prepared_train.Item1, walk_data_prepared_train.Item2);
            Train(model, train_data_prepared_train.Item1, train_data_prepared_train.Item2);
            Train(model, bike_data_prepared_train.Item1, bike_data_prepared_train.Item2);

            // model.save("models/navigation_modelV1.h5", save_format: "h5");

            var stand_result = Test(model, stand_data_prepared_test.Item1, stand_data_prepared_test.Item2);
            var walk_result = Test(model, walk_data_prepared_test.Item1, walk_data_prepared_test.Item2);
            var train_result = Test(model, train_data_prepared_test.Item1, train_data_prepared_test.Item2);
            var bike_result = Test(model, bike_data_prepared_test.Item1, bike_data_prepared_test.Item2);

            //var loaded_model = tf.keras.models.load_model("models/navigation_model.h5");
            //loaded_model.summary();

            Console.WriteLine("");
            
            Console.WriteLine($"Stand Accuracy: {stand_result["accuracy"]}");
            Console.WriteLine($"Walk Accuracy: {walk_result["accuracy"]}");
            Console.WriteLine($"Train Accuracy: {train_result["accuracy"]}");
            Console.WriteLine($"Bike Accuracy: {bike_result["accuracy"]}");

            Console.ReadLine();
        }

        public Dictionary<string, float> Test(Model model, NDArray test_data, NDArray desired_results)
        {
            return model.evaluate(test_data, desired_results);
        }

        public void Train(Model model, NDArray train_data, NDArray train_labels)
        {
            model.fit(train_data, train_labels, epochs: epochs);
        }

        public Model BuildModel()
        {
            // Create Model
            //var inputs = tf.keras.Input(shape: (4, 3));
            //var x = tf.keras.layers.LSTM(64, return_sequences: true).Apply(inputs);
            //var x2 = tf.keras.layers.LSTM(64, return_sequences: true).Apply(x);
            //var x3 = tf.keras.layers.LSTM(64).Apply(x2);
            //var outputs = tf.keras.layers.Dense(2, tf.keras.activations.Sigmoid).Apply(x3);
            //var model = tf.keras.Model(inputs, outputs);

            var layers = keras.layers;
            var myLayers = new List<ILayer>
            {
                layers.InputLayer((DataRow.SequenceLength,DataRow.InputSize)),
                layers.LSTM(64, return_sequences: true),
                layers.LSTM(64),
                layers.Dense(DataRow.OutputSize, keras.activations.Sigmoid)
            };

            Model model = keras.Sequential(myLayers);

            //var inputs = new InputLayer(new InputLayerArgs() { InputShape = (4, 3) });
            //var x = new LSTM(new LSTMArgs() { Units = 64 });
            //var outputs = new Dense(new DenseArgs() { Units = 2, Activation = new Activations().Sigmoid });
            //var model = new Sequential(new SequentialArgs());
            //model.Layers.Add(inputs);
            //model.Layers.Add(x);
            //model.Layers.Add(outputs);

            model.compile(
                loss: tf.keras.losses.MeanSquaredError(),
                optimizer: tf.keras.optimizers.Adam(),
                metrics: new[] { "accuracy" });
            model.summary();

            return model;
        }

        public static (List<DataRow>, List<DataRow>) LoadData(string csvPath, float test_ratio, bool nullPos = false)
        {
            string[] lines = File.ReadAllLines($"data/{csvPath}");

            // Skip Header
            lines = lines.Skip(1).ToArray();

            var splitIndex = (int)Math.Round(lines.Length * (1 - test_ratio));
            string[] train_rows = lines.Take(splitIndex).ToArray();
            string[] test_rows = lines.Skip(splitIndex).ToArray();

            List<DataRow> train_data = LoadRawData(train_rows, nullPos);
            List<DataRow> test_data = LoadRawData(test_rows, nullPos);

            DataRow.Export(train_data, $"data/train/{csvPath}");
            DataRow.Export(test_data, $"data/test/{csvPath}");

            return (train_data, test_data);
        }

        public static List<DataRow> LoadRawData(string[] rawData, bool nullPos = false)
        {
            const int ACCELEROMETER_X = 0;
            const int ACCELEROMETER_Y = 1;
            const int ACCELEROMETER_Z = 2;

            const int MAGNETOMETER_X = 3;
            const int MAGNETOMETER_Y = 4;
            const int MAGNETOMETER_Z = 5;

            const int GYROSCOPE_X = 6;
            const int GYROSCOPE_Y = 7;
            const int GYROSCOPE_Z = 8;

            const int POSITION_LATITUDE = 9;
            const int POSITION_LONGITUDE = 10;
            // const int POSITION_ACCURACY = 11;

            const int DATE_TIME = 12;

            List<Vector3> accelerometer = new List<Vector3>();
            List<Vector3> gyroscope = new List<Vector3>();
            List<Vector3> magnetometer = new List<Vector3>();
            List<Vector3> lastMeasurements = new List<Vector3>();
            List<Vector2?> position = new List<Vector2?>();
            List<DateTimeOffset> timestamps = new List<DateTimeOffset>();

            Vector2 startPosition = new Vector2();
            Vector3 timeSinceLastMeasurement = new Vector3(0f, 0f, 0f);



            // Remove all rows from start until there's a row with a coordinate
            int startRowIndex = 0;
            foreach (var line in rawData)
            {
                string[] values = line.Split(',');
                if (!string.IsNullOrEmpty(values[POSITION_LATITUDE]))
                {
                    startPosition = new Vector2(DataRow.ParseFloat(values[POSITION_LATITUDE]), DataRow.ParseFloat(values[POSITION_LONGITUDE]));
                    break;
                }
                startRowIndex++;
            }
            rawData = rawData.Skip(startRowIndex).ToArray();

            // Remove all rows from end until there's a row with a coordinate
            int endRowIndex = 0;
            foreach (var line in Enumerable.Reverse(rawData).ToArray())
            {
                string[] values = line.Split(',');
                if (!string.IsNullOrEmpty(values[POSITION_LATITUDE]))
                {
                    break;
                }
                endRowIndex++;
            }
            rawData = rawData.SkipLast(endRowIndex).ToArray();

            foreach (string line in rawData)
            {
                string[] values = line.Split(',');
                DateTimeOffset timestamp = DateTimeOffset.Parse(values[DATE_TIME]);

                if (timestamps.Count > 0)
                {
                    float timeDifference = (float)timestamp.Subtract(timestamps[timestamps.Count - 1]).TotalMilliseconds;
                    timeSinceLastMeasurement.X += timeDifference;
                    timeSinceLastMeasurement.Y += timeDifference;
                    timeSinceLastMeasurement.Z += timeDifference;
                    Vector3 lastMeasurement = new Vector3(timeSinceLastMeasurement.X, timeSinceLastMeasurement.Y, timeSinceLastMeasurement.Z);
                    lastMeasurements.Add(lastMeasurement);
                }
                else
                {
                    lastMeasurements.Add(timeSinceLastMeasurement);
                }
                timestamps.Add(timestamp);

                if (string.IsNullOrEmpty(values[ACCELEROMETER_X]))
                {
                    accelerometer.Add(
                        new Vector3(0f, 0f, 0f)
                    );
                }
                else
                {
                    timeSinceLastMeasurement.X = 0f;
                    accelerometer.Add(
                        new Vector3(DataRow.ParseFloat(values[ACCELEROMETER_X]), DataRow.ParseFloat(values[ACCELEROMETER_Y]), DataRow.ParseFloat(values[ACCELEROMETER_Z]))
                    );
                }

                if (string.IsNullOrEmpty(values[MAGNETOMETER_X]))
                {
                    magnetometer.Add(
                        new Vector3(0f, 0f, 0f)
                    );
                }
                else
                {
                    timeSinceLastMeasurement.Z = 0f;
                    magnetometer.Add(
                        new Vector3(DataRow.ParseFloat(values[MAGNETOMETER_X]), DataRow.ParseFloat(values[MAGNETOMETER_Y]), DataRow.ParseFloat(values[MAGNETOMETER_Z]))
                    );
                }


                if (string.IsNullOrEmpty(values[GYROSCOPE_X]))
                {
                    gyroscope.Add(
                        new Vector3(0f, 0f, 0f)
                    );
                }
                else
                {
                    timeSinceLastMeasurement.Y = 0f;
                    gyroscope.Add(
                        new Vector3(DataRow.ParseFloat(values[GYROSCOPE_X]), DataRow.ParseFloat(values[GYROSCOPE_Y]), DataRow.ParseFloat(values[GYROSCOPE_Z]))
                    );
                }

                if (string.IsNullOrEmpty(values[POSITION_LATITUDE]))
                {
                    position.Add(null);
                }
                else
                {
                    if (nullPos)
                    {
                        position.Add(
                            new Vector2(0f, 0f)
                        );
                    }
                    else
                    {
                        position.Add(
                            new Vector2(DataRow.ParseFloat(values[POSITION_LATITUDE]) - startPosition.X, DataRow.ParseFloat(values[POSITION_LONGITUDE]) - startPosition.Y)
                        );
                    }
                }
            }

            if (position[0] == null)
            {
                throw new Exception("First location must be valid!");
            }

            if (position[position.Count - 1] == null)
            {
                throw new Exception("Last location must be valid!");
            }

            List<DataRow> rows = new List<DataRow>();

            for (int i = 0; i < position.Count; i++)
            {
                if (position[i] == null)
                {
                    int startIdx = i - 1;
                    int endIdx = i + 1;

                    // Suche nach dem vorherigen vorhandenen Wert
                    while (startIdx >= 0 && position[startIdx] == null)
                    {
                        startIdx--;
                    }

                    // Suche nach dem nächsten vorhandenen Wert
                    while (endIdx < position.Count && position[endIdx] == null)
                    {
                        endIdx++;
                    }

                    if (startIdx < 0 || endIdx >= position.Count)
                    {
                        // Keine vorherigen oder nächsten Werte vorhanden, Interpolation nicht möglich
                        throw new Exception("Code has bug.");
                    }

                    Vector2 startVector = position[startIdx].Value;
                    Vector2 endVector = position[endIdx].Value;
                    int numValuesToInterpolate = endIdx - startIdx + 1;

                    // Lineare Interpolation
                    for (int j = startIdx + 1; j < endIdx; j++)
                    {
                        float interpolatedX = startVector.X + (endVector.X - startVector.X) * (j - startIdx) / numValuesToInterpolate;
                        float interpolatedY = startVector.Y + (endVector.Y - startVector.Y) * (j - startIdx) / numValuesToInterpolate;
                        position[j] = new Vector2(interpolatedX, interpolatedY);
                    }
                }
                DataRow row = new DataRow(accelerometer[i], gyroscope[i], magnetometer[i], lastMeasurements[i], (Vector2)position[i]);
                rows.Add(row);
            }

            // DataRow.Export(rows, "processed.csv");

            return rows;
        }

        public (NDArray, NDArray) PrepareData(List<DataRow> data)
        {
            var timeSteps = data.Count;
            var inputSequenceLength = DataRow.SequenceLength;

            // Prepare Train Data
            var trainData = new float[timeSteps, inputSequenceLength, DataRow.InputSize];
            for (int i = 0; i < timeSteps; i++)
            {
                for (int j = 0; j < inputSequenceLength; j++)
                {
                    var inputVector = data[i].GetInputVector(j);
                    trainData[i, j, 0] = inputVector.X;
                    trainData[i, j, 1] = inputVector.Y;
                    trainData[i, j, 2] = inputVector.Z;
                }
            }
            var train_data = new NDArray(trainData);
            Console.WriteLine($"Input Shape: {train_data.shape}");

            // Prepare Train Labels
            var trainLabels = new float[timeSteps, 1, DataRow.OutputSize];
            for (int i = 0; i < timeSteps; i++)
            {
                var outputVector = data[i].Position;
                trainLabels[i, 0, 0] = outputVector.X;
                trainLabels[i, 0, 1] = outputVector.Y;
            }
            var train_labels = new NDArray(trainLabels);
            Console.WriteLine($"Output Shape: {train_labels.shape}");

            return (train_data, train_labels);

            //train_data = new NDArray(new[, ,]
            //{ // Inputs of Time
            //    { // Vectors of Input
            //        { 1f, 2f, 3f },
            //        { 4f, 5f, 6f },
            //        { 7f, 8f, 9f },
            //        { 10f, 11f, 12f }
            //    },
            //    { // Vectors of Input
            //        { 13f, 14f, 15f },
            //        { 16f, 17f, 18f },
            //        { 19f, 20f, 21f },
            //        { 22f, 23f, 24f }
            //    },
            //    { // Vectors of Input
            //        { 25f, 26f, 27f },
            //        { 28f, 29f, 30f },
            //        { 31f, 32f, 33f },
            //        { 34f, 35f, 36f }
            //    }
            //    // Weitere Trainingsdaten hier einfügen...
            //});

            //train_labels = new NDArray(new[, ,]
            //{ // Outputs of Time
            //    { { 1f, 0f } },
            //    { { 0f, 1f } },
            //    { { 1f, 1f } }
            //    // Weitere Trainingslabels hier einfügen...
            //});
            //Console.WriteLine($"Output Shape: {train_labels.shape}");

            //test_data = new NDArray(new[, ,]
            //{ // Inputs of Time
            //    { // Vectors of Input
            //        { 37f, 38f, 39f },
            //        { 40f, 41f, 42f },
            //        { 43f, 44f, 45f },
            //        { 46f, 47f, 48f }
            //    },
            //    { // Vectors of Input
            //        { 49f, 50f, 51f },
            //        { 52f, 53f, 54f },
            //        { 55f, 56f, 57f },
            //        { 0f, 0f, 0f } // Beispiel für Masking
            //    }
            //    // Weitere Testdaten hier einfügen...
            //});

            //desired_results = new NDArray(new[,]
            //{ // Outputs of Time
            //    { 1f, 0f },
            //    { 0f, 1f }
            //    // Weitere gewünschte Ergebnisse hier einfügen...
            //});
        }
    }
}
