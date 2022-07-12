using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.SCommands;

public class BotCommands : ApplicationCommandModule
{
    private readonly InfoOptions _infoOptions;
    private readonly IGuildProvider _guildProvider;

    public BotCommands(IOptions<InfoOptions> options, IGuildProvider guildProvider)
    {
        _infoOptions = options.Value;
        _guildProvider = guildProvider;
    }

    [SlashCommand("bot", "Get info about bot. Version, source code, etc")]
    public async Task InfoCommandAsync(InteractionContext ctx)
    {
        const string content = "This bot is developed by the community.\n" +
                               "We will be happy if you join the development and help us further improve it.";

        DiscordGuild guild = ctx.Guild ?? await _guildProvider.GetCurrentGuildAsync();
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
        if (!string.IsNullOrEmpty(_infoOptions.VersionSuffix))
        {
            version += "-" + _infoOptions.VersionSuffix;
        }

        embed.AddField("Version:", version);

        var message = new DiscordInteractionResponseBuilder()
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
                    _infoOptions.ChangelogUrl,
                    "News",
                    false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":part_alternation_mark:"))
                )
            )
            .AsEphemeral(false);

        await ctx.CreateResponseAsync(message);
    }

    [SlashCommand("ping", "pong?")]
    public async Task PingCommandAsync(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync($"Pong! Latency: {ctx.Client.Ping} ms");
    }
}
