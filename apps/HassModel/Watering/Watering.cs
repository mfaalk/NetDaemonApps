using System.Threading.Tasks;
using Discord.WebSocket;
using HomeAssistantGenerated;
using HomeAssistantGenerated.Options;
using Microsoft.Extensions.Options;
using NetDaemon.HassModel.Entities;
using NetDaemonApps.Discord;
using NetDaemonApps.Options;

namespace NetDaemonApps.apps.HassModel.Watering;

[NetDaemonApp]
public class Watering
{
    private readonly IEntities _entities;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IOptions<WateringOptions> _options;

    public Watering(DiscordSocketClient discordSocketClient, IOptions<WateringOptions> options, IEntities entities)
    {
        _discordSocketClient = discordSocketClient;
        _options = options;
        _entities = entities;

        SetupSubscriptions();
    }

    private void SetupSubscriptions()
    {
        _entities.Sun.Sun.StateAllChanges()
            .Where(x => x.New.Attributes.Rising != x.Old.Attributes.Rising)
            .Where(x => x.New.Attributes.Rising == true)
            .Subscribe(SunIsRising);

        _entities.InputBoolean.IsWatering.StateAllChanges()
            .SubscribeAsync(async x => await LogIsWateringChanged(x));
        
        _entities.InputBoolean.IsWatering.StateAllChanges()
            .WhenStateIsFor(x => x.IsOn(), TimeSpan.FromMinutes(_options.Value.MaxWateringMinutes))
            .SubscribeAsync(async x => await ForceStopWateringAsync(x.Entity));
    }

    private async Task LogIsWateringChanged(StateChange<InputBooleanEntity, EntityState<InputBooleanAttributes>> stateChange) =>
        await _discordSocketClient
            .GetGuild(DiscordGuid.Id)
            .GetTextChannel(DiscordChannel.Watering)
            .SendMessageAsync(
                $"{stateChange.Entity.Attributes.FriendlyName} changed from {stateChange.Old.State} to {stateChange.New.State}");

    private async Task ForceStopWateringAsync(InputBooleanEntity wateringEntity)
    {
        wateringEntity.TurnOff();
        
        await _discordSocketClient
            .GetGuild(DiscordGuid.Id)
            .GetTextChannel(DiscordChannel.Warnings)
            .SendMessageAsync($"Turned off {wateringEntity.Attributes.FriendlyName} because it was on for {_options.Value.MaxWateringMinutes} minutes");
    }
    
    private void SunIsRising(StateChange<SunEntity, EntityState<SunAttributes>> stateChange)
    {
        if (_entities.InputBoolean.GiveWaterNextSunrise.IsOff())
        {
            return;
        }
        
        _entities.InputBoolean.IsWatering.TurnOn();
    }
}