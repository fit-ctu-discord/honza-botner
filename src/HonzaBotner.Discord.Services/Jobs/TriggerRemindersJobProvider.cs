using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hangfire;
using HonzaBotner.Database;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJobProvider : IRecurringJobProvider
    {
        private readonly IServiceScopeFactory _factory;

        private readonly ILogger<TriggerRemindersJobProvider> _logger;

        private readonly ReminderOptions _options;

        private readonly DiscordWrapper _discord;

        private readonly IGuildProvider _guild;

        public TriggerRemindersJobProvider(
            IServiceScopeFactory factory,
            ILogger<TriggerRemindersJobProvider> logger,
            IOptions<ReminderOptions> options,
            DiscordWrapper discord,
            IGuildProvider guild
        )
        {
            _factory = factory;
            _logger = logger;
            _options = options.Value;
            _discord = discord;
            _guild = guild;
        }

        public string GetKey() => "reminders-trigger";

        public string GetCronExpression() => Cron.Minutely();

        public async Task Run()
        {
            using var scope = _factory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IRemindersService>();
            var reminders = await service.DeleteRemindersThatShouldBeExecutedAsync();

            reminders.ForEach(reminder => SendReminderNotification(reminder, service));
        }

        private async void SendReminderNotification(Reminder reminder, IRemindersService service)
        {
            try
            {
                var channel = await _discord.Client.GetChannelAsync(reminder.ChannelId);
                var message = await channel.GetMessageAsync(reminder.MessageId);
                var emoji = DiscordEmoji.FromUnicode(_options.JoinEmojiName);

                var receivers = await message.GetReactionsAsync(emoji, 100);
                var mentions = $"<@{reminder.OwnerId}> " +
                               string.Join(", ", receivers.Where(user => !user.IsBot).Select(user => user.Mention));

                var embed = await CreateReminderEmbed(reminder);

                await channel.SendMessageAsync(mentions, embed: embed);
                await message.ModifyAsync("", CreateExpiredEmbed());
                await message.DeleteAllReactionsAsync();
            }
            catch (Exception exception)
            {
                _logger.LogCritical($"Exception during reminder trigger: {exception.Message}");
            }
        }

        private async Task<DiscordEmbed> CreateReminderEmbed(Reminder reminder)
        {
            var guild = await _guild.GetCurrentGuildAsync();

            return new DiscordEmbedBuilder()
                .WithTitle("🔔 " + reminder.Title.RemoveDiscordMentions(guild))
                .WithDescription(reminder.Content?.RemoveDiscordMentions(guild) ?? "")
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
