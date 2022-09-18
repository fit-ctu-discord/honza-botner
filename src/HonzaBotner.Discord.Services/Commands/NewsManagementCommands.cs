using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Services.Jobs;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Commands;

[SlashCommandGroup("news", "Commands to work with news.")]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
[SlashCommandPermissions(Permissions.ManageChannels)]
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
    public async Task ListConfigCommandAsync(InteractionContext ctx)
    {
        IList<NewsConfig> configs = await _configService.ListConfigsAsync(false).ConfigureAwait(false);

        DiscordEmbedBuilder builder = new() { Title = "News List" };
        builder.WithTimestamp(DateTime.Now);

        StringBuilder stringBuilder = new("\n");

        foreach (NewsConfig config in configs)
        {
            string active = GetActiveEmoji(config);
            stringBuilder.Append($"{active} {config.Name} [{config.Id}]");
            stringBuilder.Append($"Last fetched: {config.LastFetched}");
            stringBuilder.AppendLine();
        }


        var interaction = ctx.Client.GetInteractivity();
        var pages = interaction.GeneratePagesInEmbed(stringBuilder.ToString(), SplitType.Line, builder).ToList();

        if (!pages.Any())
        {
            await ctx.CreateResponseAsync("No configs set", true);
            return;
        }


        await interaction.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
    }


    [SlashCommand("detail", "Gets detail of one config")]
    public async Task DetailConfigCommandAsync(InteractionContext ctx,
        [Option("id", "Id of News config")] long id)
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

        await ctx.CreateResponseAsync(builder.Build());
    }

    private static string GetActiveEmoji(NewsConfig config) => config.Active ? ":white_check_mark:" : "❌";

    [SlashCommand("toggle", "Toggles if one configuration for news source is active or not")]
    public async Task ToggleConfigCommandAsync(InteractionContext context,
        [Option("id", "Id of News config")] long id)
    {
        bool currentState = await _configService.ToggleConfig(id);

        string prefix = currentState ? "" : "in";

        await context.CreateResponseAsync($"News config with id [{id}] set to be {prefix}active");
    }

    [SlashCommand("add", "Add new news configuration")]
    public async Task AddConfigCommandAsync(InteractionContext ctx,
        [Option("name", "Name of the news config")] string name,
        [Option("source", "Source identification")] string source,
        [Option("channels", "Channels where news will be published")] string channels,
        [Option("since", "Date and time of oldest news fetched")] string rawDateTime)
    {
        DateTime? parsedDateTime = ParseDateTime(rawDateTime);

        if (parsedDateTime is null)
        {
            await ctx.CreateResponseAsync("Invalid datetime format", true);
            return;
        }

        NewsConfig config = new(default, name, source, parsedDateTime.Value, NewsProviderType.Courses, PublisherType.DiscordEmbed,
            true, ctx.ResolvedChannelMentions.Select(ch => ch.Id).ToArray());

        await _configService.AddOrUpdate(config);
        await ctx.CreateResponseAsync("Success", true);
    }

    [SlashCommand("edit-channels", "Set channels for one config")]
    public async Task EditChannelsConfigCommandAsync(InteractionContext ctx,
        [Option("id", "Id of News config")] long id,
        [Option("channels", "Channels where news will be published")] string channels)
    {
        NewsConfig config = await _configService.GetById(id);

        config = config with { Channels = ctx.ResolvedChannelMentions.Select(ch => ch.Id).ToArray() };

        await _configService.AddOrUpdate(config);
        await ctx.CreateResponseAsync("Success", true);
    }

    [SlashCommand("edit-last-run", "Set last run time for one config")]
    public async Task EditLastRunConfigCommandAsync(InteractionContext ctx,
        [Option("id", "Id of News config")] long id,
        [Option("last-run", "Date and time of last run")] string rawDateTime)
    {
        DateTime? lastRun = ParseDateTime(rawDateTime);

        if (lastRun is null)
        {
            await ctx.CreateResponseAsync("Invalid last-run format");
            return;
        }

        NewsConfig config = await _configService.GetById(id);

        config = config with { LastFetched = lastRun.Value };

        await _configService.AddOrUpdate(config);
        await ctx.CreateResponseAsync("Success", true);
    }

    [SlashCommand("run-once", "Fetch news once")]
    public async Task RunOnceCommandAsync(InteractionContext ctx)
    {
        await _newsJobProvider.ExecuteAsync(default);
        await ctx.CreateResponseAsync("News job - done");
    }

    private static DateTime? ParseDateTime(string datetime)
    {
        if (DateTime.TryParse(datetime, new CultureInfo("cs-CZ"), DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsed))
        {
            return parsed;
        }

        return new Parser().Parse(datetime)?.Start;
    }
}
