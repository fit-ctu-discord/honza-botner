using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Scheduler.Contract;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Jobs
{
    [Cron("*/5 * * * * *")]
    public class TriggerRemindersJobProvider : IJob
    {
        private readonly IRemindersService _remindersService;

        private readonly ILogger<TriggerRemindersJobProvider> _logger;

        private readonly ReminderOptions _reminderOptions;

        private readonly DiscordWrapper _discord;

        private readonly IReminderManager _reminderManager;

        public TriggerRemindersJobProvider(
            IRemindersService remindersService,
            ILogger<TriggerRemindersJobProvider> logger,
            IOptions<ReminderOptions> options,
            DiscordWrapper discord,
            IReminderManager reminderManager
        )
        {
            _remindersService = remindersService;
            _logger = logger;
            _reminderOptions = options.Value;
            _discord = discord;
            _reminderManager = reminderManager;
        }

        public string Name { get; } = "reminders-trigger";

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now; // Fix one point in time.
            var reminders = await _remindersService.GetRemindersToExecuteAsync(now);
            await _remindersService.DeleteExecutedRemindersAsync(now);

            foreach (var reminder in reminders)
                await SendReminderNotification(reminder);
        }

        private async Task SendReminderNotification(Reminder reminder)
        {
            try
            {
                DiscordChannel channel = await _discord.Client.GetChannelAsync(reminder.ChannelId);
                DiscordMessage message = await channel.GetMessageAsync(reminder.MessageId);
                DiscordEmoji emoji = DiscordEmoji.FromUnicode(_reminderOptions.JoinEmojiName);

                // Get receivers from reactions + reminder owner.
                var receiversFromReactions = await message.GetReactionsAsync(emoji);
                var receivers = receiversFromReactions.Where(user => !user.IsBot)
                    .Select(u => u.Id).ToList();
                receivers.Add(reminder.OwnerId);

                DiscordEmbed embed = await _reminderManager.CreateDmReminderEmbedAsync(reminder);

                // DM all users.
                foreach (ulong user in receivers)
                {
                    DiscordMember? member;
                    try
                    {
                        member = await message.Channel.Guild.GetMemberAsync(user);
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning("Couldn't find user with id {Id}", user);
                        continue;
                    }

                    DiscordDmChannel dmChannel = await member.CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync(embed: embed);
                }

                DiscordEmbed expiredEmbed = await _reminderManager.CreateExpiredReminderEmbedAsync(reminder);

                // Expire old reaction message.
                await message.ModifyAsync("", expiredEmbed);
                await message.DeleteAllReactionsAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception during reminder trigger: {Message}", e.Message);
            }
        }
    }
}
