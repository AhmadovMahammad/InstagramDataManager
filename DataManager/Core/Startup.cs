using Microsoft.Extensions.DependencyInjection;

namespace DataManager.Core;
internal static class Startup
{
    public static void ConfigureServices(IServiceCollection serviceDescriptors)
    {
        // Register application services
        serviceDescriptors.AddSingleton<ApplicationRunner>();
        serviceDescriptors.AddSingleton<ICommandHandler, CommandHandler>();
    }
}