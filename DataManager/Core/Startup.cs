using DataManager.Core.Services.Contracts;
using DataManager.Core.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace DataManager.Core;
internal static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ApplicationRunner>();

        services.AddSingleton<ICommandResolver, CommandResolver>();
        services.AddSingleton<ILoginService, LoginService>();
    }
}