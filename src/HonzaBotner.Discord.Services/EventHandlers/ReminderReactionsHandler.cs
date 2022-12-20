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
using DiscordRole = DSharpPlus.Entities.DiscordRole;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class ReminderReactionsHandler : IEventHandler<MessageReactionAddEventArgs>
{
    private readonly CommonCommandOptions _options;
    private readonly ReminderOptions _reminderOptions;

    private readonly IRemindersService _service;

    private readonly IReminderManager _reminderManager;

    public ReminderReactionsHandler(
        IOptions<CommonCommandOptions> commonOptions,
        IOptions<ReminderOptions> options,
        IRemindersService service,
        IReminderManager reminderManager
    )
    {
        _options = commonOptions.Value;
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
        if (emoji == _reminderOptions.CancelEmojiName)
        {
            bool isMod = false;
            if (arguments.Guild is not null)
            {
                DiscordRole moderatorRole = arguments.Guild.GetRole(_options.ModRoleId);
                DiscordMember discordMember = await arguments.Guild.GetMemberAsync(arguments.User.Id);
                isMod = discordMember.Roles.Contains(moderatorRole);
            }

            if (reminder.OwnerId == arguments.User.Id || isMod)
            {
                await _service.DeleteReminderAsync(reminder.Id);
                await arguments.Message.ModifyAsync("",
                    await _reminderManager.CreateCanceledReminderEmbedAsync(reminder));

                return EventHandlerResult.Stop;
            }
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
