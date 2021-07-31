using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class ReminderReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
    {
        private readonly ReminderOptions _options;

        private readonly IRemindersService _service;

        public ReminderReactionsHandler(IOptions<ReminderOptions> options, IRemindersService service)
        {
            _options = options.Value;
            _service = service;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs arguments)
        {
            var emoji = arguments.Emoji.Name;
            var validEmojis = new[] { _options.CancelEmojiName, _options.JoinEmojiName };

            if (validEmojis.Contains<>(validEmojis))
            {
                return EventHandlerResult.Continue;
            }

            // TODO: Move this to framework level code?
            if (arguments.User.IsBot)
            {
                return EventHandlerResult.Stop;
            }

            var reminder = await _service.GetByMessageIdAsync(arguments.Message.Id);

            if (reminder == null)
            {
                return EventHandlerResult.Continue;
            }

            // The owner has cancelled the reminder
            if (emoji == _options.CancelEmojiName && arguments.User.Id == reminder.OwnerId)
            {
                await _service.DeleteReminderAsync(reminder);
                await arguments.Message.ModifyAsync("", CreateCancelledReminderEmbed());
                await arguments.Message.DeleteAllReactionsAsync("Reminder cancelled");

                return EventHandlerResult.Stop;
            }

            // Somebody else wants to be mentioned within the reminder notification
            if (emoji == _options.JoinEmojiName && arguments.User.Id != reminder.OwnerId)
            {
                // There is nothing that has to be done, as the reactions are checked during the reminder notification
                // This check is just to disable owner from joining his own reminder
                return EventHandlerResult.Stop;
            }

            // Otherwise just remove the emoji...
            await arguments.Message.DeleteReactionAsync(arguments.Emoji, arguments.User, "It is not a valid reaction.");
            return EventHandlerResult.Continue;
        }

        private static DiscordEmbed CreateCancelledReminderEmbed()
        {
            return new DiscordEmbedBuilder()
                .WithTitle("Reminder cancelled.")
                .WithTimestamp(DateTime.Now);
        }
    }
}
