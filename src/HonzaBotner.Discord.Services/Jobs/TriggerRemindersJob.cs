using System;
using System.Linq;
using System.Threading.Tasks;
using Chronic.Core.System;
using DSharpPlus.Entities;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.EventHandlers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJob
    {
        private readonly IRemindersService _service;

        private readonly ILogger<TriggerRemindersJob> _logger;

        private readonly ReminderOptions _options;

        private readonly DiscordWrapper _discord;

        public TriggerRemindersJob(
            IRemindersService service,
            ILogger<TriggerRemindersJob> logger,
            IOptions<ReminderOptions> options,
            DiscordWrapper discord)
        {
            _service = service;
            _logger = logger;
            _options = options.Value;
            _discord = discord;
        }

        public async Task Dispatch()
        {
            var reminders = await _service.GetRemindersThatShouldBeExecutedAsync();

            reminders.ForEach(SendReminderNotification);
        }

        private async void SendReminderNotification(Reminder reminder)
        {
            try
            {
                var channel = await _discord.Client.GetChannelAsync(reminder.ChannelId);
                var message = await channel.GetMessageAsync(reminder.MessageId);
                var emoji = DiscordEmoji.FromUnicode(_options.JoinEmojiName);

                var receivers = await message.GetReactionsAsync(emoji, 100);
                var mentions = string.Join(", ", receivers.Select(user => user.Mention) + $"<@{reminder.OwnerId}>");

                await channel.SendMessageAsync(mentions, embed: CreateReminderEmbed(reminder));
                await message.ModifyAsync("Reminder expired");
                await message.DeleteAllReactionsAsync();
                await _service.CancelReminderAsync(reminder);
            }
            catch (Exception exception)
            {
                _logger.LogCritical("Exception during reminder trigger", exception);
            }
        }

        private static DiscordEmbed CreateReminderEmbed(Reminder reminder)
        {
            return new DiscordEmbedBuilder()
                .WithTitle("🔔 " + reminder.Title)
                .WithDescription(reminder.Content ?? "")
                .WithColor(new DiscordColor("#ffac33"))
                .WithTimestamp(reminder.DateTime)
                .Build();
        }
    }
}
