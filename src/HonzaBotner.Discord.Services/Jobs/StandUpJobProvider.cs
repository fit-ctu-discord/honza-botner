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
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 0 8 * * ?")]
public class StandUpJobProvider : IJob
{
    private readonly ILogger<StandUpJobProvider> _logger;

    private readonly DiscordWrapper _discord;

    private readonly CommonCommandOptions _commonOptions;

    private readonly IStandUpStatsService _statsService;

    private IGuildProvider _guildProvider;

    public StandUpJobProvider(
        ILogger<StandUpJobProvider> logger,
        DiscordWrapper discord,
        IOptions<CommonCommandOptions> commonOptions,
        IStandUpStatsService statsService,
        IGuildProvider guildProvider
    )
    {
        _logger = logger;
        _discord = discord;
        _commonOptions = commonOptions.Value;
        _statsService = statsService;
        _guildProvider = guildProvider;
    }

    /// <summary>
    /// Stand-up task regex.
    ///
    /// state:
    /// [] - in progress during the day, failed later
    /// [:white_check_mark:] - completed
    ///
    /// priority:
    /// [] - normal
    /// []! - critical
    /// </summary>
    private static readonly Regex TaskRegex = new(@"^ *\[ *(?<State>\S*) *\] *(?<Priority>[!])?",
        RegexOptions.Multiline);

    private static readonly List<string> OkList = new() { "check", "done", "ok", "✅" };

    public string Name => "standup";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Today; // Fix one point in time.
        DateTime yesterday = today.AddDays(-1);

        try
        {
            DiscordChannel channel = await _discord.Client.GetChannelAsync(_commonOptions.StandUpChannelId);

            var ok = new StandUpStats();
            var fail = new StandUpStats();

            List<DiscordMessage> messageList = new();
            messageList.AddRange(await channel.GetMessagesAsync());

            while (messageList.LastOrDefault()?.Timestamp.Date == yesterday)
            {
                int messagesCount = messageList.Count;
                messageList.AddRange(
                    await channel.GetMessagesBeforeAsync(messageList.Last().Id)
                );

                // No new data.
                if (messageList.Count <= messagesCount)
                {
                    break;
                }
            }

            HashSet<ulong> membersToDm = new();

            foreach (DiscordMessage msg in messageList.Where(msg => msg.Timestamp.Date == yesterday))
            {
                int total = 0;
                int completed = 0;

                foreach (Match match in TaskRegex.Matches(msg.Content))
                {
                    total++;
                    string state = match.Groups["State"].ToString();
                    string priority = match.Groups["Priority"].ToString();

                    if (OkList.Any(s => state.Contains(s)))
                    {
                        completed++;
                        ok.Increment(priority);
                    }
                    else
                    {
                        fail.Increment(priority);
                    }
                }

                // Update DB.
                await _statsService.UpdateStats(msg.Author.Id, completed, total);
                StandUpStat? stats = await _statsService.GetStats(msg.Author.Id);

                if (stats is null)
                {
                    _logger.LogWarning("No stats presented for member {Member}", msg.Author.Mention);
                    continue;
                }

                // Send DM to the current member (only once).
                if (membersToDm.Contains(msg.Author.Id))
                {
                    continue;
                }

                membersToDm.Add(msg.Author.Id);

                try
                {
                    DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
                    DiscordMember member = await guild.GetMemberAsync(msg.Author.Id);

                    // Send DM to the member.
                    string heading = await _statsService.IsValidStreak(msg.Author.Id)
                        ? "Skvělá práce!"
                        : "Nějak ti to nevyšlo...";
                    await member.SendMessageAsync($@"
{heading} Včera jsi splnil {completed} z {total} tasků a jsi momentálně na streaku {stats.Streak} s {stats.Freezes} možnými freezes.

Celkově jsi splnil {stats.TotalCompleted} z {stats.TotalTasks} tasků a nejdelší streak jsi měl {stats.LongestStreak} dní.
");
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Could not send a message to {Member}", msg.Author.Mention);
                }
            }

            // Send stats message to channel.
            await channel.SendMessageAsync($@"
Stand-up time, <@&{_commonOptions.StandUpRoleId}>!

Results from <t:{((DateTimeOffset)today.AddDays(-1)).ToUnixTimeSeconds()}:D>:
```
all:        {ok.Add(fail)}
completed:  {ok}
failed:     {fail}
```
");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during standup trigger: {Message}", e.Message);
        }
    }
}
