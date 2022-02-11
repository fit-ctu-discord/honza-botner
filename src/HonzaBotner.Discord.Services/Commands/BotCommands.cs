using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands;

[Group("bot")]
[Description("Commands to get you info about Discord bot and other things.")]
public class BotCommands : BaseCommandModule
{
    private readonly ILogger<BotCommands> _logger;
    private readonly InfoOptions _infoOptions;
    private readonly IGuildProvider _guildProvider;

    public BotCommands(
        ILogger<BotCommands> logger,
        IOptions<InfoOptions> infoOptions,
        IGuildProvider guildProvider
    )
    {
        _logger = logger;
        _infoOptions = infoOptions.Value;
        _guildProvider = guildProvider;
    }

    [Command("activity")]
    [Description("Changes bot's activity status.")]
    [RequireMod]
    [RequireGuild]
    public async Task Activity(CommandContext ctx,
        [Description("Type of the activity. Allowed values: competing, playing, watching or listeningTo.")]
        ActivityType type,
        [Description("Status value of the activity."), RemainingText]
        string status
    )
    {
        if (type is ActivityType.Custom or ActivityType.Streaming)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            return;
        }

        try
        {
            DiscordActivity activity = new() { ActivityType = type, Name = status };

            await ctx.Client.UpdateStatusAsync(activity);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Couldn't update bot's status");
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
        }
    }

    [Command("info")]
    [Description("Command to get you info about Discord bot and other things.")]
    [GroupCommand]
    public async Task BotInfo(CommandContext ctx)
    {
        string content = "This bot is developed by the community.\n" +
                         "We will be happy if you join the development and help us further improve it.";

        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        DiscordMember bot = await guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

        try
        {
            DiscordEmbedBuilder embed = new()
            {
                Author =
                    new DiscordEmbedBuilder.EmbedAuthor { Name = bot.DisplayName, IconUrl = bot.AvatarUrl },
                Title = "Information about the bot",
                Description = content,
                Color = DiscordColor.CornflowerBlue
            };
            string version =
                Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? "<unknown version>";
            if (!string.IsNullOrEmpty(_infoOptions.VersionSuffix))
            {
                version += "-" + _infoOptions.VersionSuffix;
            }

            embed.AddField("Version:", version);

            DiscordMessageBuilder message = new DiscordMessageBuilder()
                .AddEmbed(embed)
                .AddComponents(
                    new DiscordLinkButtonComponent(
                        _infoOptions.RepositoryUrl,
                        "Source code",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":scroll:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _infoOptions.IssueTrackerUrl,
                        "Report an issue",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":bug:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _infoOptions.RepositoryUrl + "/tree/main/docs",
                        "Documentation",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":book:"))
                    ),
                    new DiscordLinkButtonComponent(
                        _infoOptions.ChangelogUrl,
                        "News",
                        false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":part_alternation_mark:"))
                    )
                );

            await ctx.Channel.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Couldn't build and send a embed message");
        }
    }
}
