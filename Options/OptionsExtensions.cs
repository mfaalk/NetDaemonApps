using HomeAssistantGenerated.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemonApps.Options;

public static class OptionsExtensions
{
    public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WateringOptions>(configuration.GetSection(nameof(WateringOptions)));
        return services;
    }
}