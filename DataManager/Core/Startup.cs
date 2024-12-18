using DataManager.Factories;
using DataManager.Handlers.DisplayHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace DataManager.Core;
internal static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register application services
        services.AddSingleton<ApplicationRunner>();
        services.AddSingleton<ICommandHandler, CommandHandler>();
        services.AddTransient<IOperationHandler, DisplayFollowersHandler>();
    }
}