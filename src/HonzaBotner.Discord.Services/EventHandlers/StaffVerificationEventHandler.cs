using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Discord.Utils;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers;

public class StaffVerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
{
    private readonly IUrlProvider _urlProvider;
    private readonly ButtonOptions _buttonOptions;
    private readonly DiscordRoleConfig _discordRoleConfig;
    private readonly IDiscordRoleManager _roleManager;
    private readonly ILogger<StaffVerificationEventHandler> _logger;
    private readonly ITranslation _translation;

    public StaffVerificationEventHandler(IUrlProvider urlProvider,
        IOptions<DiscordRoleConfig> discordRoleConfig,
        IDiscordRoleManager roleManager,
        ILogger<StaffVerificationEventHandler> logger,
        IOptions<ButtonOptions> buttonConfig,
        ITranslation translation
    )
    {
        _urlProvider = urlProvider;
        _buttonOptions = buttonConfig.Value;
        _discordRoleConfig = discordRoleConfig.Value;
        _roleManager = roleManager;
        _logger = logger;
        _translation = translation;
    }

    public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
    {
        if (eventArgs.Id != _buttonOptions.StaffVerificationId && eventArgs.Id != _buttonOptions.StaffRemoveRoleId)
        {
            return EventHandlerResult.Continue;
        }

        if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
        {
            _translation.SetLanguage(ITranslation.Language.Czech);
        }

        DiscordUser user = eventArgs.User;
        DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
        var builder = new DiscordInteractionResponseBuilder().AsEphemeral();

        // Check if the button to remove staff roles was pressed.
        if (eventArgs.Id == _buttonOptions.StaffRemoveRoleId)
        {
            bool revoked = await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
            builder.Content = _translation["RolesSuccessfullyDeleted"];

            if (!revoked)
            {
                _logger.LogInformation(
                    "Ungranting roles for user {Username} (id {Id}) failed",
                    eventArgs.User.Username,
                    eventArgs.User.Id
                );
                builder.Content = "Staff roles failed to remove. Please contact the moderators.";
            }

            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
            return EventHandlerResult.Stop;
        }

        // Check if the user is authenticated.
        bool isAuthenticated = _discordRoleConfig.AuthenticatedRoleIds.Any(
            roleId => member.Roles.Select(role => role.Id).Contains(roleId)
        );

        if (!isAuthenticated)
        {
            string verificationLink = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);
            builder.Content = _translation["UserNotVerified"];
            builder.AddComponents(
                new DiscordLinkButtonComponent(
                    verificationLink,
                    _translation["VerifyRolesButton"],
                    false,
                    new DiscordComponentEmoji("✅")
                )
            );
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                builder);

            return EventHandlerResult.Stop;
        }

        // Check if the user already has some staff roles
        bool isStaffAuthenticated = false;
        foreach (ulong[] roleIds in _discordRoleConfig.StaffRoleMapping.Values)
        {
            if (roleIds.Any(roleId => member.Roles.Select(role => role.Id).Contains(roleId)))
            {
                isStaffAuthenticated = true;
            }
        }

        string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Staff);

        if (isStaffAuthenticated && _buttonOptions.StaffRemoveRoleId is not null)
        {
            builder.Content = _translation["AlreadyVerified"];
            builder.AddComponents(
                new DiscordLinkButtonComponent(
                    link,
                    _translation["UpdateStaffRolesButton"],
                    false,
                    new DiscordComponentEmoji("👑")
                ),
                new DiscordButtonComponent(
                    ButtonStyle.Danger,
                    _buttonOptions.StaffRemoveRoleId,
                    _translation["RemoveRolesButton"],
                    false,
                    new DiscordComponentEmoji("🗑️")
                )
            );
        }
        else
        {
            builder.Content = _translation["VerifyStaff"];
            builder.AddComponents(new DiscordLinkButtonComponent(
                link,
                _translation["VerifyStaffRolesButton"],
                false,
                new DiscordComponentEmoji("👑"))
            );
        }

        await eventArgs.Interaction.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            builder
        );

        return EventHandlerResult.Stop;
    }
}
