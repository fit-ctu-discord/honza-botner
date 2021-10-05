using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("bot", "Příkazy pro získání informací o botovi a jeho základní nastavení")]
    public class BotSCommands : ApplicationCommandModule
    {
        private readonly ILogger<BotSCommands> _logger;
        private readonly InfoOptions _options;

        public BotSCommands(ILogger<BotSCommands> logger, IOptions<InfoOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        [SlashCommand("activity", "Aktivita kterou bot zobrazí na svém profilu", false)]
        [SlashRequireUserPermissions(Permissions.ManageGuild, false)]
        public async Task ActivityAsync(InteractionContext ctx,
            [Option("Aktivita", "Nazev vykonavane aktivity")] string message,
            [Option("Druh", "Druh aktivity ktera se zobrazi u bota")]
            [Choice("Competing", "Competing")]
            [Choice("ListeningTo", "ListeningTo")]
            [Choice("Playing", "Playing")]
            [Choice("Watching", "Watching")]
            string slashActivityType = "Competing")
        {
            ActivityType type = slashActivityType switch
            {
                "Competing" => ActivityType.Competing,
                "ListeningTo" => ActivityType.ListeningTo,
                "Playing" => ActivityType.Playing,
                "Watching" => ActivityType.Watching,
                _ => throw new ArgumentException("Unknown activity type", slashActivityType)
            };

            DiscordActivity activity = new (message, type);
            // Commented for debugging reasons. It's better to catch exact error instead of all
            // try
            // {
                await ctx.Client.UpdateStatusAsync(activity, UserStatus.Online);
                DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                    .WithContent($"Aktivita nastavena na {slashActivityType}: {message}");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
            // }
            // catch (Exception e)
            // {
            //     _logger.LogWarning("Couldn't set activity to {SlashActivityType}: {Message}",
            //         slashActivityType, message);
            //     DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            //         .WithContent($"Chyba: nepovedlo se nastavit aktivitu na {slashActivityType}: {message}")
            //         .AsEphemeral(true);
            //     await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
            // }
        }

        [SlashCommand("info", "Zakaldni informace o botovi")]
        public async Task InfoAsync(InteractionContext ctx)
        {
            return;
            // TODO: wait for #201 merge
        }
    }
}
