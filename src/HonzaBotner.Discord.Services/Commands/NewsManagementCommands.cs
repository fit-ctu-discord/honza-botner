using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Jobs;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("news", "Commands to work with news.")]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
[SlashCommandPermissions(Permissions.ModerateMembers)]
public class NewsManagementCommands : ApplicationCommandModule
{
    private readonly INewsConfigService _configService;
    private readonly NewsJobProvider _newsJobProvider;

    public NewsManagementCommands(INewsConfigService configService, NewsJobProvider newsJobProvider)
    {
        _configService = configService;
        _newsJobProvider = newsJobProvider;
    }

    [SlashCommand("list", "List all news configs")]
    public async Task ListConfig(InteractionContext context)
    {
        IList<NewsConfig> configs = await _configService.ListConfigsAsync(false).ConfigureAwait(false);

        DiscordEmbedBuilder builder = new() { Title = "News List" };
        builder.WithTimestamp(DateTime.Now);
        foreach (NewsConfig config in configs)
        {
            string active = GetActiveEmoji(config);
            builder.AddField($"{active} {config.Name} [{config.Id}]",
                $"Last fetched: {config.LastFetched}");
        }

        await context.Channel.SendMessageAsync(builder.Build());
    }


    [SlashCommand("detail", "Gets detail of one config")]
    public async Task DetailConfig(InteractionContext context,
        [Option("id", "Id of News config")] int id)
    {
        NewsConfig config = await _configService.GetById(id);

        string active = GetActiveEmoji(config);
        DiscordEmbedBuilder builder = new() { Title = $"{active} {config.Name} [{config.Id}]" };
        string[] channels = config.Channels.Select(chId => $"<#{chId}>").ToArray();

        builder.AddField("Source", config.Source);
        builder.AddField("Channels", string.Join(',', channels));
        builder.AddField("Last fetched", config.LastFetched.ToString(CultureInfo.InvariantCulture));
        builder.AddField("Sourcing news via", config.NewsProvider.ToString());
        builder.AddField("Publishing news via", config.Publisher.ToString());

        builder.WithTimestamp(DateTime.Now);

        await context.CreateResponseAsync(builder.Build());
    }

    private static string GetActiveEmoji(NewsConfig config) => config.Active ? ":white_check_mark:" : "❌";

    [SlashCommand("toggle", "Toggles if one configuration for news source is active or not")]
    public async Task ToggleConfig(InteractionContext context,
        [Option("id", "Id of News config")] int id)
    {
        bool currentState = await _configService.ToggleConfig(id);

        string prefix = currentState ? "" : "in";

        await context.CreateResponseAsync($"News config with id [{id}] set to be {prefix}active");
    }

    [SlashCommand("add", "Add new news configuration")]
    public async Task AddConfig(InteractionContext context,
        [Option("name", "Name of the news config")] string name,
        [Option("source", "Source identification")] string source,
        [Option("channels", "Channels where news will be published")]
        params DiscordChannel[] channels)
    {
        NewsConfig config = new(default, name, source, DateTime.MinValue, NewsProviderType.Courses, PublisherType.DiscordEmbed,
            true, channels.Select(ch => ch.Id).ToArray());

        await _configService.AddOrUpdate(config);
    }

    [SlashCommand("edit-channels", "Set channels for one config")]
    public async Task EditChannelsConfig(InteractionContext context,
        [Option("id", "Id of News config")] int id,
        [Option("channels", "Channels where news will be published")]
        params DiscordChannel[] channels)
    {
        NewsConfig config = await _configService.GetById(id);

        config = config with { Channels = channels.Select(ch => ch.Id).ToArray() };

        await _configService.AddOrUpdate(config);
    }

    [SlashCommand("edit-last-run", "Set last run time for one config")]
    public async Task EditLastRunConfig(InteractionContext context,
        [Option("id", "Id of News config")] int id, DateTime lastRun)
    {
        NewsConfig config = await _configService.GetById(id);

        config = config with { LastFetched = lastRun };

        await _configService.AddOrUpdate(config);
    }

    [SlashCommand("run-once", "Fetch news once")]
    public async Task RunOnce(InteractionContext context)
    {
        await _newsJobProvider.ExecuteAsync(default);
        await context.CreateResponseAsync("News job - done");
    }
}
