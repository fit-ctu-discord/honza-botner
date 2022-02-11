using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class ReminderReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{
    private readonly ReminderOptions _reminderOptions;

    private readonly IRemindersService _service;

    private readonly IReminderManager _reminderManager;

    public ReminderReactionsHandler(
        IOptions<ReminderOptions> options,
        IRemindersService service,
        IReminderManager reminderManager
    )
    {
        _reminderOptions = options.Value;
        _service = service;
        _reminderManager = reminderManager;
    }

    public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs arguments)
    {
        if (arguments.User.IsBot)
        {
            return EventHandlerResult.Stop;
        }

        DiscordEmoji emoji = arguments.Emoji;
        string[] validEmojis = { _reminderOptions.CancelEmojiName, _reminderOptions.JoinEmojiName };
        if (!validEmojis.Contains(emoji))
        {
            return EventHandlerResult.Continue;
        }

        Reminder? reminder = await _service.GetByMessageIdAsync(arguments.Message.Id);
        if (reminder == null)
        {
            return EventHandlerResult.Continue;
        }

        // The owner has canceled the reminder
        if (emoji == _reminderOptions.CancelEmojiName && arguments.User.Id == reminder.OwnerId)
        {
            await _service.DeleteReminderAsync(reminder.Id);
            await arguments.Message.ModifyAsync("",
                await _reminderManager.CreateCanceledReminderEmbedAsync(reminder));

            return EventHandlerResult.Stop;
        }

        // Somebody else wants to be mentioned within the reminder notification
        if (emoji == _reminderOptions.JoinEmojiName && arguments.User.Id != reminder.OwnerId)
        {
            // There is nothing that has to be done, as the reactions are checked during the reminder notification
            // This check is just to disable owner from joining his own reminder
            return EventHandlerResult.Stop;
        }

        // Otherwise just remove the emoji...
        await arguments.Message.DeleteReactionAsync(arguments.Emoji, arguments.User, "It is not a valid reaction.");
        return EventHandlerResult.Stop;
    }
}
