using Discord.WebSocket;
using HomeAssistantGenerated.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemonApps.Discord;

public static class DiscordExtensions
{
    public static IServiceCollection AddDiscord(this IServiceCollection services, IConfiguration configuration,
        Action<DiscordSocketConfig>? config = null)
    {
        config ??= _ => { };
            
        services
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .Configure<DiscordOptions>(configuration.GetSection(nameof(DiscordOptions)))
            .AddHostedService<InitializeDiscordService>();
        
        return services;
    }
}