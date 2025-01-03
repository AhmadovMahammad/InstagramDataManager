﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataManager.Core;
internal class Program
{
    private static IHost _host = null!;

    private static void Main(string[] args)
    {
        using Mutex mutex = new Mutex(true, @"Global\InstagramDataManager");

        try
        {
            // Try to acquire the mutex for up to 3 seconds.
            if (!mutex.WaitOne(TimeSpan.FromSeconds(3)))
            {
                Console.WriteLine("Another instance of the app is running! Press Any Key To Exit...");
                Console.ReadKey();
                return;
            }

            _host = CreateHostBuilder(args).Build();
            RunApplication();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                Startup.ConfigureServices(services);
            });
    }

    private static void RunApplication()
    {
        var applicationRunner = _host.Services.GetRequiredService<ApplicationRunner>();
        applicationRunner.Run();
    }
}