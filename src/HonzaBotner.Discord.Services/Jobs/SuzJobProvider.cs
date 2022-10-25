using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Helpers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Scheduler.Contract;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 30 10 * * 1-5")]
public class SuzJobProvider : IJob
{
    private readonly ILogger<SuzJobProvider> _logger;

    private readonly DiscordWrapper _discord;
    private readonly ICanteenService _canteenService;
    private readonly IGuildProvider _guildProvider;
    private readonly CommonCommandOptions _commonOptions;

    public SuzJobProvider(
        ILogger<SuzJobProvider> logger,
        DiscordWrapper discord,
        ICanteenService canteenService,
        IGuildProvider guildProvider,
        IOptions<CommonCommandOptions> commonOptions)
    {
        _logger = logger;
        _discord = discord;
        _canteenService = canteenService;
        _guildProvider = guildProvider;
        _commonOptions = commonOptions.Value;
    }

    public string Name => "suz-agata";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        IList<CanteenDto> canteens = await _canteenService.ListCanteensAsync(true, cancellationToken);

        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        guild.ListActiveThreadsAsync()
        _discord.Client.SendMessageAsync()
    }
}
