using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hangfire;
using HonzaBotner.Discord.Extensions;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Jobs
{
    public class TriggerRemindersJobProvider : IRecurringJobProvider
    {
        private readonly IServiceScopeFactory _factory;

        private readonly ILogger<TriggerRemindersJobProvider> _logger;

        private readonly ReminderOptions _reminderOptions;

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
            _reminderOptions = options.Value;
            _discord = discord;
            _guild = guild;
        }

        public const string Key = "reminders-trigger";

        public static string CronExpression => Cron.Minutely();

        public static string Test { get; set; }

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

                DiscordEmbed embed = await CreateReminderEmbed(reminder);

                // DM all users.
                foreach (DiscordUser user in receivers)
                {
                    if (!message.Channel.Guild.Members.ContainsKey(user.Id)) continue;

                    DiscordDmChannel dmChannel = await message.Channel.Guild.Members[user.Id].CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync(embed: embed);
                }

                DiscordEmbed expiredEmbed = await CreateExpiredEmbed(reminder);

                // Delete reminder from DB.
                await service.DeleteReminderAsync(reminder.Id);

                // Expire old reaction message.
                await message.ModifyAsync("", expiredEmbed);
                await message.DeleteAllReactionsAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception during reminder trigger: {Message}", e.Message);
            }
        }

        private async Task<DiscordEmbed> CreateReminderEmbed(Reminder reminder)
        {
            var guild = await _guild.GetCurrentGuildAsync();

            return CreateAbstractEmbed(
                "🔔 Reminder from FIT ČVUT Discord",
                guild.Members.ContainsKey(reminder.OwnerId)
                    ? guild.Members[reminder.OwnerId]
                    : null,
                reminder.Content.RemoveDiscordMentions(guild),
                DiscordColor.Yellow,
                reminder.DateTime
            );
        }

        private async Task<DiscordEmbed> CreateExpiredEmbed(Reminder reminder)
        {
            var guild = await _guild.GetCurrentGuildAsync();

            return CreateAbstractEmbed(
                    "🔕 Expired reminder",
                    guild.Members.ContainsKey(reminder.OwnerId)
                        ? guild.Members[reminder.OwnerId]
                        : null,
                    reminder.Content.RemoveDiscordMentions(guild),
                    DiscordColor.Grayple,
                    DateTime.Now
                );
        }

        private static DiscordEmbed CreateAbstractEmbed(
            string title,
            DiscordMember? author,
            string description,
            DiscordColor color,
            DateTime dateTime
        )
        {
            return new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithAuthor(
                    author?.RatherNicknameThanUsername() ?? "Unknown user",
                    iconUrl: author?.AvatarUrl)
                .WithDescription(description)
                .WithColor(color)
                .WithTimestamp(dateTime)
                .Build();
        }
    }
}
