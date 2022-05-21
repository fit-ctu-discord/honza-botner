using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class StandUpHandler : IEventHandler<ComponentInteractionCreateEventArgs>
{

    private readonly ButtonOptions _buttonOptions;
    private readonly IStandUpStatsService _standUpStats;
    private readonly ILogger<StandUpHandler> _logger;

    public StandUpHandler(
        IOptions<ButtonOptions> buttonOptions,
        IStandUpStatsService standUpStats,
        ILogger<StandUpHandler> logger)
    {
        _buttonOptions = buttonOptions.Value;
        _standUpStats = standUpStats;
        _logger = logger;
    }

    public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs args)
    {
        if (args.Id != _buttonOptions.StandUpStatsId)
        {
            return EventHandlerResult.Continue;
        }

        _logger.LogDebug("{User} requested stats info", args.User.Username);

        StandUpStat? stats = await _standUpStats.GetStreak(args.User.Id);

        DiscordInteractionResponseBuilder response = new();
        response.AsEphemeral(true);
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor("Stats", iconUrl: args.User.AvatarUrl);

        if (stats is null)
        {
            embed.Description = "Unfortunately you are not in the database yet.\nDatabase updates daily at 8 am";
            embed.Color = new Optional<DiscordColor>(DiscordColor.Gold);
            response.AddEmbed(embed.Build());

            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
            return EventHandlerResult.Stop;
        }

        embed.Description = $"Cool stats for {args.User.Mention}";
        embed.AddField("Current streak", stats.Streak.ToString(), true);
        embed.AddField("Available freezes", stats.Freezes.ToString(), true);
        embed.AddField("Total tasks", stats.TotalTasks.ToString(), true);
        embed.AddField("Total completed tasks", stats.TotalCompleted.ToString(), true);
        embed.AddField("Last streak update",
            "<t:" + ((DateTimeOffset)stats.LastDayOfStreak.AddDays(1)).ToUnixTimeSeconds() + ":D>");
        embed.Color = new Optional<DiscordColor>(DiscordColor.Wheat);

        response.AddEmbed(embed.Build());
        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        return EventHandlerResult.Stop;
    }

}
