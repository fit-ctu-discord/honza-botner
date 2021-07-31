using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hangfire;
using HonzaBotner.Database;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJobProvider : IRecurringJobProvider
    {
        private readonly IRemindersService _service;

        private readonly ILogger<TriggerRemindersJobProvider> _logger;

        private readonly ReminderOptions _options;

        private readonly DiscordWrapper _discord;

        public TriggerRemindersJobProvider(
            IRemindersService service,
            ILogger<TriggerRemindersJobProvider> logger,
            IOptions<ReminderOptions> options,
            DiscordWrapper discord)
        {
            _service = service;
            _logger = logger;
            _options = options.Value;
            _discord = discord;
        }

        public string GetKey() => "reminders-trigger";

        public string GetCronExpression() => Cron.Minutely();

        public async Task Run()
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
                var mentions = $"<@{reminder.OwnerId}> " +
                               string.Join(", ", receivers.Where(user => !user.IsBot).Select(user => user.Mention));

                await channel.SendMessageAsync(mentions, embed: CreateReminderEmbed(reminder));
                await message.ModifyAsync("", CreateExpiredEmbed());
                await message.DeleteAllReactionsAsync();
                await _service.DeleteReminderAsync(reminder);
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"Exception during reminder trigger: {exception.Message}");
            }
        }

        private static DiscordEmbed CreateReminderEmbed(Reminder reminder)
        {
            return new DiscordEmbedBuilder()
                .WithTitle("🔔 " + reminder.Title)
                .WithDescription(reminder.Content ?? "")
                .WithColor(new DiscordColor("ffac33"))
                .WithTimestamp(reminder.DateTime)
                .Build();
        }

        private static DiscordEmbed CreateExpiredEmbed()
        {
            return new DiscordEmbedBuilder()
                .WithTitle("Reminder expired")
                .WithColor(DiscordColor.Blurple)
                .WithTimestamp(DateTime.Now)
                .Build();
        }
    }
}
