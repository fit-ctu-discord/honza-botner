using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class StandupButtonHandler : IEventHandler<ComponentInteractionCreateEventArgs>
{

    private readonly ButtonOptions _buttonOptions;
    private readonly CommonCommandOptions _commandOptions;

    private readonly ILogger<StandupButtonHandler> _logger;

    public StandupButtonHandler(
        IOptions<ButtonOptions> buttonOptions,
        IOptions<CommonCommandOptions> commandOptions,
        ILogger<StandupButtonHandler> logger)
    {
        _buttonOptions = buttonOptions.Value;
        _commandOptions = commandOptions.Value;
        _logger = logger;
    }


    public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs args)
    {
        if (args.Id != _buttonOptions.StandupSwitchPingId)
        {
            return EventHandlerResult.Continue;
        }

        DiscordMember target = await args.Guild.GetMemberAsync(args.User.Id);
        DiscordRole standupPingRole = args.Guild.GetRole(_commandOptions.StandUpRoleId);
        StringBuilder response = new ();

        try
        {
            if (target.Roles.Contains(standupPingRole))
            {
                await target.RevokeRoleAsync(standupPingRole, "Doesn't want to be pinged anymore");
                response.Append("🚫 Successfully removed ping role");
            }
            else
            {
                await target.GrantRoleAsync(standupPingRole, "Wants to be pinged");
                response.Append("✅ Successfully added ping role");
            }
        }
        catch (DiscordException e)
        {
            _logger.LogWarning(e, "Failed while managing standup roles.");
            response.Append("🤖 Error occured, please contact moderators");
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(response.ToString()).AsEphemeral());

        return EventHandlerResult.Stop;
    }


}
