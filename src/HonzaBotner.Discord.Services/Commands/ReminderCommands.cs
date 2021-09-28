using System;
using System.Globalization;
using System.Threading.Tasks;
using Chronic.Core;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Extensions;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("reminder")]
    [Aliases("remind")]
    [Description("Commands to manager reminders.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Cooldown(2, 60 * 60, CooldownBucketType.User)]
    [RequireGuild]
    public class ReminderCommands : BaseCommandModule
    {
        private readonly IRemindersService _service;

        private readonly ReminderOptions _options;

        private readonly IReminderManager _reminderManager;

        public ReminderCommands(
            IRemindersService service,
            IOptions<ReminderOptions> options,
            IReminderManager reminderManager
        )
        {
            _service = service;
            _options = options.Value;
            _reminderManager = reminderManager;
        }

        [GroupCommand]
        [Command("create")]
        [Aliases("me")] // Allows a more "fluent" usage ::remind me <>
        [Description("Create a new reminder.")]
        public async Task Create(
            CommandContext context,
            [Description("Date or time of the reminder")]
            string rawDatetime,
            [Description("Content of the reminder."), RemainingText]
            string? content
        )
        {
            DateTime now = DateTime.Now;
            DateTime? datetime = ParseDateTime(rawDatetime);

            if (content == null)
            {
                await context.RespondErrorAsync(
                    $"Cannot parse content string",
                    "You didn't provide any content for the reminder."
                );
                await context.Message.DeleteAsync();
                return;
            }

            if (datetime == null)
            {
                await context.RespondErrorAsync(
                    $"Cannot parse datetime string `{rawDatetime}`",
                    "Try using an explicit datetime or expressions like `in 30 minutes`, `tomorrow at 8:00`, ..."
                );
                return;
            }

            if (datetime <= now)
            {
                await context.RespondErrorAsync(
                    "Cannot schedule reminders in the past.",
                    "You can only create reminders that are in the future."
                );
                return;
            }

            await context.TriggerTypingAsync();
            DiscordMessage message = await context.Channel.SendMessageAsync("Creating reminder...");

            Reminder reminder = await _service.CreateReminderAsync(
                context.User.Id,
                message.Id,
                message.ChannelId,
                datetime.Value, // This is safe, as the nullability is validated above
                content
            );

            await message.ModifyAsync("", await _reminderManager.CreateReminderEmbedAsync(reminder));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.CancelEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode(_options.JoinEmojiName));
            await context.Message.DeleteAsync();
        }

        private static DateTime? ParseDateTime(string datetime)
        {
            if (DateTime.TryParse(datetime, new CultureInfo("cs-CZ"), DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsed))
            {
                return parsed;
            }

            return new Parser().Parse(datetime)?.Start;
        }
    }
}
