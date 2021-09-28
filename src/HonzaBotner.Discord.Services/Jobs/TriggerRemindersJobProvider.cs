using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hangfire;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJobProvider : IRecurringJobProvider
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

        public const string Key = "reminders-trigger";

        public static string CronExpression => Cron.Minutely();

        public async Task Run()
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
                var receivers = receiversFromReactions.Where(user => !user.IsBot);
                if (message.Channel.Guild.Members.ContainsKey(reminder.OwnerId))
                {
                    receivers = receivers.Append(message.Channel.Guild.Members[reminder.OwnerId]);
                }

                DiscordEmbed embed = await _reminderManager.CreateDmReminderEmbedAsync(reminder);

                // DM all users.
                foreach (DiscordUser user in receivers)
                {
                    if (!message.Channel.Guild.Members.ContainsKey(user.Id)) continue;

                    DiscordDmChannel dmChannel = await message.Channel.Guild.Members[user.Id].CreateDmChannelAsync();
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
