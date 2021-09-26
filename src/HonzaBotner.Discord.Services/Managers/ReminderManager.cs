using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.Managers
{
    public class ReminderManager : IReminderManager
    {
        private readonly IGuildProvider _guildProvider;

        public ReminderManager(IGuildProvider guildProvider)
        {
            _guildProvider = guildProvider;
        }


        public async Task<DiscordEmbed> CreateReminderEmbedAsync(Reminder reminder)
        {
            var guild = await _guildProvider.GetCurrentGuildAsync();

            return _CreateEmbed(
                "🔔 Reminder from FIT ČVUT Discord",
                guild.Members.ContainsKey(reminder.OwnerId)
                    ? guild.Members[reminder.OwnerId]
                    : null,
                reminder.Content.RemoveDiscordMentions(guild),
                DiscordColor.Yellow,
                reminder.DateTime
            );
        }

        public async Task<DiscordEmbed> CreateExpiredReminderEmbedAsync(Reminder reminder)
        {
            var guild = await _guildProvider.GetCurrentGuildAsync();

            return _CreateEmbed(
                "🔕 Expired reminder",
                guild.Members.ContainsKey(reminder.OwnerId)
                    ? guild.Members[reminder.OwnerId]
                    : null,
                reminder.Content.RemoveDiscordMentions(guild),
                DiscordColor.Grayple
            );
        }

        public async Task<DiscordEmbed> CreateCanceledReminderEmbedAsync(Reminder reminder)
        {
            var guild = await _guildProvider.GetCurrentGuildAsync();

            return _CreateEmbed(
                "🛑 Canceled reminder",
                guild.Members.ContainsKey(reminder.OwnerId)
                    ? guild.Members[reminder.OwnerId]
                    : null,
                reminder.Content.RemoveDiscordMentions(guild),
                DiscordColor.Red
            );
        }

        private static DiscordEmbed _CreateEmbed(
            string title,
            DiscordMember? author,
            string description,
            DiscordColor color,
            DateTime? dateTime = null
        )
        {
            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithAuthor(
                    author?.RatherNicknameThanUsername() ?? "Unknown user",
                    iconUrl: author?.AvatarUrl)
                .WithDescription(description)
                .WithColor(color);

            if (dateTime != null)
            {
                embedBuilder.WithTimestamp(dateTime);
            }

            return embedBuilder.Build();
        }
    }
}
