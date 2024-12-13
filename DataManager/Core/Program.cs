using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataManager.Core;
internal class Program
{
    private static IHost _host = null!;

    private static void Main(string[] args)
    {
        CreateHostBuilder(args);

        GetRequiredService<ApplicationRunner>().Run();
    }

    private static void CreateHostBuilder(string[] args)
    {
        _host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, serviceCollection) =>
            {
                Startup.ConfigureServices(serviceCollection);
            }).Build();
    }

    private static T GetRequiredService<T>() where T : class => _host.Services.GetRequiredService<T>();
}