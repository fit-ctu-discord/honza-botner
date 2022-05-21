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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 0 8 * * ?")]
public class StandUpJobProvider : IJob
{
    private readonly ILogger<StandUpJobProvider> _logger;

    private readonly DiscordWrapper _discord;

    private readonly StandUpOptions _standUpOptions;
    private readonly ButtonOptions _buttonOptions;

    private readonly IStandUpStatsService _statsService;

    public StandUpJobProvider(
        ILogger<StandUpJobProvider> logger,
        DiscordWrapper discord,
        IOptions<StandUpOptions> standUpOptions,
        IStandUpStatsService statsService,
        IOptions<ButtonOptions> buttonOptions
    )
    {
        _logger = logger;
        _discord = discord;
        _standUpOptions = standUpOptions.Value;
        _statsService = statsService;
        _buttonOptions = buttonOptions.Value;
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

    private static readonly List<string> OkList = new() { "check", "done", "ok", "✅", "x" };

    public string Name => "standup";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime yesterday = DateTime.Today.AddDays(-1);

        try
        {
            DiscordChannel channel = await _discord.Client.GetChannelAsync(_standUpOptions.StandUpChannelId);

            var ok = new StandUpStats();
            var fail = new StandUpStats();

            List<DiscordMessage> messageList = new();
            messageList.AddRange((await channel.GetMessagesAsync()).Where(msg => !msg.Author.IsBot));

            while (messageList.LastOrDefault()?.Timestamp.Date == yesterday)
            {
                int messagesCount = messageList.Count;
                messageList.AddRange(
                    (await channel.GetMessagesBeforeAsync(messageList.Last().Id)).Where(msg => !msg.Author.IsBot)
                );

                // No new data.
                if (messageList.Count <= messagesCount)
                {
                    break;
                }
            }

            foreach (var authorGrouped in messageList.Where(msg => msg.Timestamp.Date == yesterday)
                         .GroupBy(msg => msg.Author.Id))
            {
                int total = 0;
                int completed = 0;

                foreach (var msg in authorGrouped)
                {

                    foreach (Match match in TaskRegex.Matches(msg.Content))
                    {
                        total++;
                        string state = match.Groups["State"].Value;
                        string priority = match.Groups["Priority"].Value;

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
                }

                // Update DB.
                if (total != 0)
                {
                    await _statsService.UpdateStats(authorGrouped.Key, completed, total);
                }
            }

            // Send stats message to channel.
            DiscordMessageBuilder message = new DiscordMessageBuilder().WithContent($@"
Stand-up time, <@&{_standUpOptions.StandUpRoleId}>!

Results from <t:{((DateTimeOffset)yesterday).ToUnixTimeSeconds()}:D>:
```
all:        {ok.Add(fail)}
completed:  {ok}
failed:     {fail}
```
");
            if (_buttonOptions.StandUpStatsId is not null)
            {
                message.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, _buttonOptions.StandUpStatsId,
                    "Get your stats", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🐸"))));
            }

            await message.SendAsync(channel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during standup trigger: {Message}", e.Message);
        }
    }
}
