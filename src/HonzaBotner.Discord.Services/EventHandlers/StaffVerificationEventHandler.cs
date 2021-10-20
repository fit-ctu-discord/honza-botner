using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class StaffVerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly ButtonOptions _buttonOptions;
        private readonly DiscordRoleConfig _discordRoleConfig;
        private readonly IDiscordRoleManager _roleManager;
        private readonly ILogger<StaffVerificationEventHandler> _logger;

        private enum _Language
        {
            CS,
            EN
        }

        public StaffVerificationEventHandler(IUrlProvider urlProvider,
            IOptions<DiscordRoleConfig> discordRoleConfig,
            IDiscordRoleManager roleManager,
            ILogger<StaffVerificationEventHandler> logger,
            IOptions<ButtonOptions> buttonConfig)
        {
            _urlProvider = urlProvider;
            _buttonOptions = buttonConfig.Value;
            _discordRoleConfig = discordRoleConfig.Value;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Id != _buttonOptions.StaffVerificationId && eventArgs.Id != _buttonOptions.StaffRemoveRoleId)
            {
                return EventHandlerResult.Continue;
            }

            _Language current = _Language.EN;
            if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
            {
                current = _Language.CS;
            }

            DiscordUser user = eventArgs.User;
            DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
            var builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);

            // Check if the button to remove staff roles was pressed.
            if (eventArgs.Id == _buttonOptions.StaffRemoveRoleId)
            {
                bool revoked = await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
                builder.Content = current switch
                {
                    _Language.EN => "Role byly úspěšně odstraněny.",
                    _Language.CS => "Roles have been successfully deleted.",
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (!revoked)
                {
                    _logger.LogInformation(
                        "Ungranting roles for user {Username} (id {Id}) failed",
                        eventArgs.User.Username,
                        eventArgs.User.Id
                    );
                    builder.Content = current switch
                    {
                        _Language.EN => "Zaměstnanecké role se nepodařilo odebrat. Prosím, kontaktujte moderátory.",
                        _Language.CS => "Failed to remove staff roles. Please contact the mods.",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
                return EventHandlerResult.Stop;
            }

            // Check if the user is authenticated.
            bool isAuthenticated = false;
            foreach (ulong roleId in _discordRoleConfig.AuthenticatedRoleIds)
            {
                if (member.Roles.Select(role => role.Id).Contains(roleId))
                {
                    isAuthenticated = true;
                    break;
                }
            }

            if (!isAuthenticated)
            {
                string verificationLink = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);
                builder.Content = current switch
                {
                    _Language.EN => "Hi, you are not verified yet!\n" +
                                    "1) Click the button below to verify and assign roles according to UserMap. ✅\n" +
                                    "2) Then click the button to add employee roles again. 👑",
                    _Language.CS => "Ahoj, ještě nejsi ověřený!\n" +
                                    "1) Pro ověření a přidělení rolí dle UserMap klikni na tlačítko dole. ✅\n" +
                                    "2) Následně znovu klikni na tlačítko pro přidání zaměstnaneckých rolí. 👑",
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        verificationLink,
                        current switch
                        {
                            _Language.EN => "Verify",
                            _Language.CS => "Ověřit se",
                            _ => throw new ArgumentOutOfRangeException()
                        },
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
                foreach (ulong roleId in roleIds)
                {
                    if (member.Roles.Select(role => role.Id).Contains(roleId))
                    {
                        isStaffAuthenticated = true;
                        break;
                    }
                }
            }

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Staff);

            if (isStaffAuthenticated && _buttonOptions.StaffRemoveRoleId is not null)
            {
                builder.Content = current switch
                {
                    _Language.EN => "Hi, you are already verified.\n" +
                                    "Click the button to update employee roles.",
                    _Language.CS => "Ahoj, už jsi ověřený.\n" +
                                    "Pro aktualizaci zaměstnaneckých rolí klikni na tlačítko.",
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        current switch
                        {
                            _Language.EN => "Update staff roles",
                            _Language.CS => "Aktualizovat role zaměstnance",
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        false,
                        new DiscordComponentEmoji("👑")
                    ),
                    new DiscordButtonComponent(
                        ButtonStyle.Danger,
                        _buttonOptions.StaffRemoveRoleId,
                        current switch
                        {
                            _Language.EN => "Remove roles",
                            _Language.CS => "Odebrat role",
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        false,
                        new DiscordComponentEmoji("🗑️")
                    )
                );
            }
            else
            {
                builder.Content = current switch
                {
                    _Language.EN => "Hi, click the button to verify the staff roles.",
                    _Language.CS => "Ahoj, pro ověření rolí zaměstnance klikni na tlačítko.",
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.AddComponents(new DiscordLinkButtonComponent(
                    link,
                    current switch
                    {
                        _Language.EN => "Verify staff roles",
                        _Language.CS => "Ověřit role zaměstnance",
                        _ => throw new ArgumentOutOfRangeException()
                    },
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
}
