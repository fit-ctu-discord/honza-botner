using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("bot", "Commands to get/set bot's basic information")]
    public class BotSCommands : ApplicationCommandModule
    {
        private readonly ILogger<BotSCommands> _logger;
        private readonly InfoOptions _options;
        private readonly IGuildProvider _guildProvider;

        public BotSCommands(ILogger<BotSCommands> logger, IOptions<InfoOptions> options, IGuildProvider guildProvider)
        {
            _logger = logger;
            _options = options.Value;
            _guildProvider = guildProvider;
        }

        [SlashCommand("activity", "Activity bot displays on his profile")]
        public async Task ActivityAsync(InteractionContext ctx,
            [Option("Message", "Name of the activity")] string message,
            [Option("Type", "Activity type")]
            [Choice("Competing", "competing")]
            [Choice("ListeningTo", "listeningTo")]
            [Choice("Playing", "playing")]
            [Choice("Watching", "watching")]
            string slashActivityType = "competing")
        {
            ActivityType type = slashActivityType switch
            {
                "competing" => ActivityType.Competing,
                "listeningTo" => ActivityType.ListeningTo,
                "playing" => ActivityType.Playing,
                "watching" => ActivityType.Watching,
                _ => throw new ArgumentException("Unknown activity type", slashActivityType)
            };

            DiscordActivity activity = new (message, type);
            await ctx.Client.UpdateStatusAsync(activity, UserStatus.Online);
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .WithContent($"Activity set to {slashActivityType}: {message}");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }

        [SlashCommand("info", "Basic bot information")]
        public async Task InfoAsync(InteractionContext ctx)
        {
            string content = "This bot is developed by the community.\n" +
                             "We will be happy if you join the development and help us further improve it.";

            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
            DiscordMember bot = await guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = bot.DisplayName, IconUrl = bot.AvatarUrl },
                    Title = "Information about the bot",
                    Description = content,
                    Color = DiscordColor.CornflowerBlue
                };
            string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "<unknown version>";
            if (!string.IsNullOrEmpty(_options.VersionSuffix))
            {
                version += "-" + _options.VersionSuffix;
            }
            embed.AddField("Version:", version);

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true)
                .AddComponents(
                    new DiscordLinkButtonComponent(
                        _options.RepositoryUrl,
                        "Source code",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":scroll:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _options.IssueTrackerUrl,
                        "Report an issue",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":bug:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _options.RepositoryUrl + "/tree/main/docs",
                        "Documentation",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":book:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _options.ChangelogUrl,
                        "News",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":part_alternation_mark:"))
                    )
                );

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }
    }
}
