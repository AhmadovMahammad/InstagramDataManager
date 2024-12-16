using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataManager.Core;
internal class Program
{
    private static IHost _host = null!;

    private static void Main(string[] args)
    {
        using Mutex mutex = new Mutex(true, @"Global\InstagramDataManager");

        // Try to acquire the mutex for up to 3 seconds.
        if (!mutex.WaitOne(TimeSpan.FromSeconds(3)))
        {
            Console.WriteLine("Another instance of the app is running! Press Any Key To Exit...");
            Console.ReadKey();
            return;
        }

        CreateHostBuilder(args);
        RunApplication();
    }

    private static void CreateHostBuilder(string[] args)
    {
        _host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, serviceCollection) =>
            {
                Startup.ConfigureServices(serviceCollection);
            }).Build();
    }

    private static void RunApplication()
    {
        var applicationRunner = _host.Services.GetRequiredService<ApplicationRunner>();
        applicationRunner.Run();
    }
}