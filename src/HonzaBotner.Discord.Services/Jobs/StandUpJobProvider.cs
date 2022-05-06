using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Scheduler.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 0 8 * * ?")]
public class StandUpJobProvider : IJob
{
    private readonly ILogger<StandUpJobProvider> _logger;

    private readonly DiscordWrapper _discord;

    private readonly CommonCommandOptions _commonOptions;

    public StandUpJobProvider(
        ILogger<StandUpJobProvider> logger,
        DiscordWrapper discord,
        IOptions<CommonCommandOptions> commonOptions
    )
    {
        _logger = logger;
        _discord = discord;
        _commonOptions = commonOptions.Value;
    }

    /// <summary>
    /// Stand-up task regex.
    ///
    /// state:
    /// [] - in progress
    /// [:white_check_mark:] - completed
    /// [:x:] - failed
    ///
    /// priority:
    /// [] - normal
    /// []? - optional
    /// []! - must
    /// </summary>
    private static readonly Regex s_regex = new(@"\s*\[\s*(?<State>\S*)\s*\]\s*(?<Priority>.)");

    private static readonly List<string> s_okList = new() { "check", "ok", "✅" };
    private static readonly List<string> s_failList = new() { "fail", "no", "❌" };

    public string Name { get; } = "standup";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Today; // Fix one point in time.
        await SendStandUpNotification(today);
    }

    private async Task SendStandUpNotification(DateTime today)
    {
        DateTime yesterday = today.AddDays(-1);

        try
        {
            DiscordChannel channel = await _discord.Client.GetChannelAsync(_commonOptions.StandUpChannelId);

            List<DiscordMessage> messageList = new();
            messageList.AddRange(
                (await channel.GetMessagesAsync())
                .Where(msg => msg.Timestamp.Date == yesterday)
            );

            while (true)
            {
                int messagesCount = messageList.Count;
                messageList.AddRange(
                    (await channel.GetMessagesBeforeAsync(messageList.Last().Id))
                    .Where(msg => msg.Timestamp.Date == yesterday)
                );

                // No new data.
                if (messageList.Count <= messagesCount)
                {
                    break;
                }
            }

            var ok = new StandUpStats();
            var fail = new StandUpStats();
            var inProgress = new StandUpStats();

            foreach (DiscordMessage msg in messageList)
            {
                foreach (string line in msg.Content.Split('\n'))
                {
                    var match = s_regex.Match(line);
                    if (!match.Success)
                    {
                        continue;
                    }

                    string state = match.Groups["State"].ToString();
                    string priority = match.Groups["Priority"].ToString();

                    if (s_okList.Any(s => state.Contains(s)))
                    {
                        ok.Increment(priority);
                    }
                    else if (s_failList.Any(s => state.Contains(s)))
                    {
                        fail.Increment(priority);
                    }
                    else
                    {
                        inProgress.Increment(priority);
                    }
                }
            }

            await channel.SendMessageAsync($@"
Stand-up time, <@{_commonOptions.StandUpRoleId}>!

Results from <t:{((DateTimeOffset)today.AddDays(-1)).ToUnixTimeSeconds()}:D>:
```
all:            {ok.Sum + fail.Sum + inProgress.Sum}
completed:      {ok}
failed:         {fail}
in-progress:    {inProgress}
```
");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during standup trigger: {Message}", e.Message);
        }
    }
}

internal class StandUpStats
{
    private int _normal;
    private int _optional;
    private int _must;

    private const string Optional = "?";
    private const string Must = "!";

    public void Increment(string priority)
    {
        if (priority.Contains(Must))
        {
            _must++;
        }
        else if (priority.Contains(Optional))
        {
            _optional++;
        }
        else
        {
            _normal++;
        }
    }

    public int Sum => _normal + _optional + _must;

    public override string ToString()
    {
        return $"{Sum} ({_normal} + {_optional}? + {_must}!)";
    }
}
