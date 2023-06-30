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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs;

[Cron("0 0 8 * * ?")]
public class StandUpJobProvider : IJob
{
    private readonly ILogger<StandUpJobProvider> _logger;

    private readonly DiscordWrapper _discord;

    private readonly ButtonOptions _buttonOptions;
    private readonly CommonCommandOptions _commonOptions;

    public StandUpJobProvider(
        ILogger<StandUpJobProvider> logger,
        DiscordWrapper discord,
        IOptions<CommonCommandOptions> commonOptions,
        IOptions<ButtonOptions> buttonOptions)
    {
        _logger = logger;
        _discord = discord;
        _buttonOptions = buttonOptions.Value;
        _commonOptions = commonOptions.Value;
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
    private static readonly Regex Regex = new(@"^ *\[ *(?<State>\S*) *\] *(?<Priority>[!])?", RegexOptions.Multiline);

    private static readonly List<string> OkList = new() { "check", "done", "ok", "✅", "x" };

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

            foreach (DiscordMessage msg in messageList.Where(msg => msg.Timestamp.Date == yesterday))
            {
                foreach (Match match in Regex.Matches(msg.Content))
                {
                    string state = match.Groups["State"].ToString();
                    string priority = match.Groups["Priority"].ToString();

                    if (OkList.Any(s => state.Contains(s)))
                    {
                        ok.Increment(priority);
                    }
                    else
                    {
                        fail.Increment(priority);
                    }
                }
            }

            if(ok.Add(fail).Sum == 0)
            {
                return;
            }
            
            DiscordRole standupPingRole = channel.Guild.GetRole(_commonOptions.StandUpRoleId);

            var content = new DiscordMessageBuilder()
                .WithContent($@"
Stand-up time @here!

Results from <t:{((DateTimeOffset)today.AddDays(-1)).ToUnixTimeSeconds()}:D>:
```
all:        {ok.Add(fail)}
completed:  {ok}
failed:     {fail}
```
||{standupPingRole.Mention}||")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, _buttonOptions.StandupSwitchPingId,
                "Switch ping", emoji:new DiscordComponentEmoji("🔔")))
                .WithAllowedMention(new RoleMention(standupPingRole));
            await channel.SendMessageAsync(content);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during standup trigger: {Message}", e.Message);
        }
    }
}
