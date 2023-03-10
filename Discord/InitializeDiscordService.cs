using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HomeAssistantGenerated.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NetDaemonApps;

public class InitializeDiscordService : IHostedService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IOptions<DiscordOptions> _options;

    public InitializeDiscordService(DiscordSocketClient discordSocketClient, IOptions<DiscordOptions> options)
    {
        _discordSocketClient = discordSocketClient;
        _options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discordSocketClient.Log += async (msg) =>
        {
            await Task.CompletedTask;
            Console.WriteLine(msg);
        };

        _discordSocketClient.Ready += () => 
        {
            Console.WriteLine("Bot is connected!");
            return Task.CompletedTask;
        };

        await _discordSocketClient.LoginAsync(TokenType.Bot, _options.Value.Token);
        await _discordSocketClient.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordSocketClient.StopAsync();
    }
}