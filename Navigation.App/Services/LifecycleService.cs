namespace Navigation.App.Services
{
    internal class LifecycleService
    {
        public IService Service { get; }
        public int Count = 0;

        public LifecycleService(IService _service)
        {
            Service = _service;
        }

        public void OnCreate()
        {
            Count += 1;
            if (Count == 1)
            {
                Service.Start();
            }
        }

        public void OnDestroy()
        {
            Count -= 1;
            if (Count == 0)
            {
                Service.Stop();
                System.Environment.Exit(0);
            }
        }
    }
}
