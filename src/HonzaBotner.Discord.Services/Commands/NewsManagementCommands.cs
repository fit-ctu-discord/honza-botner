using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Jobs;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands;

[Group("news")]
[Description("Commands to interact with members.")]
[ModuleLifespan(ModuleLifespan.Transient)]
[RequireMod]
public class NewsManagementCommands : BaseCommandModule
{
    private readonly INewsConfigService _configService;
    private readonly NewsJobProvider _newsJobProvider;

    public NewsManagementCommands(INewsConfigService configService, NewsJobProvider newsJobProvider)
    {
        _configService = configService;
        _newsJobProvider = newsJobProvider;
    }

    [Command("list")]
    [GroupCommand]
    public async Task ListConfig(CommandContext context)
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


    [Command("Detail")]
    [Description("Gets detail of one config")]
    public async Task DetailConfig(CommandContext context, int id)
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

        await context.RespondAsync(builder.Build());
    }

    private static string GetActiveEmoji(NewsConfig config) => config.Active ? ":white_check_mark:" : "❌";

    [Command("toggle")]
    [Description("Toggles if one configuration for news source is active or not")]
    public async Task ToggleConfig(CommandContext context, int id)
    {
        bool currentState = await _configService.ToggleConfig(id);

        string prefix = currentState ? "" : "in";

        await context.RespondAsync($"News config with id [{id}] set to be {prefix}active");
    }

    [Command("add")]
    public async Task AddConfig(CommandContext context, string name, string source, NewsProviderType newsProviderType,
        PublisherType publisherProviderType, params DiscordChannel[] channels)
    {
        NewsConfig config = new(default, name, source, DateTime.MinValue, newsProviderType, publisherProviderType,
            true, channels.Select(ch => ch.Id).ToArray());

        await _configService.AddOrUpdate(config);
    }

    [Command("edit-channels")]
    [Description("Set channels for one config")]
    public async Task EditChannelsConfig(CommandContext context, int id, params DiscordChannel[] channels)
    {
        NewsConfig config = await _configService.GetById(id);

        config = config with { Channels = channels.Select(ch => ch.Id).ToArray() };

        await _configService.AddOrUpdate(config);
    }

    [Command("edit-last-run")]
    [Description("Set last run time for one config")]
    public async Task EditLastRunConfig(CommandContext context, int id, DateTime lastRun)
    {
        NewsConfig config = await _configService.GetById(id);

        config = config with { LastFetched = lastRun };

        await _configService.AddOrUpdate(config);
    }

    [Command("run-once")]
    public async Task RunOnce(CommandContext context)
    {
        await context.TriggerTypingAsync();
        await _newsJobProvider.ExecuteAsync(default);
        await context.RespondAsync("News job - done");
    }
}
