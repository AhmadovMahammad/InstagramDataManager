﻿using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Core;
using DataManager.Helper.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace DataManager;
internal class Program
{
    private static IHost _host = null!;

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        ConsoleExtension.PrintBanner(); // generated by: https://patorjk.com/software/taag/#p=display&f=Graffiti&t=Data%20Manager

        using Mutex mutex = new Mutex(true, @"Global\InstagramDataManager");
        bool acquiredLock = false;

        try
        {
            // Try to acquire the mutex for up to 3 seconds.
            if (!mutex.WaitOne(TimeSpan.FromSeconds(3)))
            {
                Console.WriteLine("Another instance of the app is running! Press Any Key To Exit...");
                Console.ReadKey();

                return;
            }

            acquiredLock = true;
            _host = CreateHostBuilder(args).Build();

            CreateFolderIfNeeded();
            RunApplication();
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occurred. Press any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            if (acquiredLock)
            {
                mutex.ReleaseMutex();
            }

            Console.WriteLine("Session Completed! Press Any Key To Exit...");
            Console.ReadKey();
        }
    }

    private static void CreateFolderIfNeeded()
    {
        string path = AppConstant.ApplicationDataFolderPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            $"Path will contain several important files: {path}. You can obtain data from there if you need it.".WriteMessage(MessageType.Success);
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