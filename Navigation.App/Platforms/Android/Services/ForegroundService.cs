using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading;

namespace Navigation.App.Services;

[Service]
public class DemoServices : Service, IService //we implement our service (IServiceTest) and use Android Native Service Class
{
    public static bool IsForegroundServiceRunning;
    public const string ChannelId = "ServiceChannel";
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public override IBinder OnBind(Intent intent)
    {
        throw new NotImplementedException();
    }

    [return: GeneratedEnum] // we catch the actions intents to know the state of the foreground service
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        base.OnStartCommand(intent, flags, startId);

        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sensorService = ServiceProvider.GetService<SensorService>();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                sensorService.Start();
                await sensorService.TrackGeolocation(cancellationToken);
            }
            catch (System.OperationCanceledException e)
            {
                sensorService.Stop();
                StopForeground(StopForegroundFlags.Remove); // Stop the service
                StopSelfResult(startId);
                throw e;
            }
        }, cancellationToken);

        return StartCommandResult.NotSticky;
    }

    // Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
    // Setting one action to start and one action to stop the foreground service
    public void Start()
    {
        Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(DemoServices));
        startService.SetAction("START_SERVICE");
        MainActivity.ActivityCurrent.StartService(startService);
    }

    public void Stop()
    {
        Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
        stopIntent.SetAction("STOP_SERVICE");
        MainActivity.ActivityCurrent.StartService(stopIntent);
    }

    public override void OnCreate()
    {
        base.OnCreate();
        RegisterNotification();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        cancellationTokenSource.Cancel();
    }

    private void RegisterNotification()
    {
        NotificationChannel channel = new NotificationChannel(ChannelId, "ServiceDemo", NotificationImportance.Min);
        NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
        manager.CreateNotificationChannel(channel);
        Notification notification = new Notification.Builder(this)
           .SetContentTitle("Sensor Tracking")
           .SetContentText("Required sensor data is gathered in the background...")
           .SetSmallIcon(Resource.Mipmap.appicon)
           .SetChannelId(ChannelId)
           .SetOngoing(true)
           .Build();

        StartForeground(2931, notification);
    }
}
