using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Helpers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Scheduler.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 0 8 * * ?")]
public class ModexJobProvider : IJob
{
    private readonly ILogger<ModexJobProvider> _logger;

    private readonly DiscordWrapper _discord;

    private readonly CommonCommandOptions _commonOptions;

    public ModexJobProvider(
        ILogger<ModexJobProvider> logger,
        DiscordWrapper discord,
        IOptions<CommonCommandOptions> commonOptions
    )
    {
        _logger = logger;
        _discord = discord;
        _commonOptions = commonOptions.Value;
    }

    public string Name => "modex";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DiscordChannel channel = await _discord.Client.GetChannelAsync(_commonOptions.CryClosetChannelId);

        var content = new DiscordMessageBuilder()
            // @M0dEx má dnes narozeniny! Všechno nejlepší 🥳 🎉
            .WithContent($@"<@343108790708600832> má dnes narozeniny! Všechno nejlepší :partying_face::tada:");

        await channel.SendMessageAsync(content);
    }
}
