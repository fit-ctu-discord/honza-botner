using System.Collections.Generic;
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

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class StaffVerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly ButtonOptions _buttonOptions;
        private readonly DiscordRoleConfig _discordRoleConfig;
        private readonly IDiscordRoleManager _roleManager;
        private readonly ILogger<StaffVerificationEventHandler> _logger;

        private enum _textsKeys
        {
            SuccessFullyDeleted,
            CouldNotDelete,
            NotVerifiedYet,
            VerifyBtn,
            AlreadyVerified,
            UpdateStaffRolesBtn,
            RemoveRolesBtn,
            VerifyStaff,
            VerifyStaffRolesBtn
        }

        private readonly Dictionary<_textsKeys, Dictionary<Language, string>> _texts = new()
        {
            [_textsKeys.SuccessFullyDeleted] =
            {
                [Language.Czech] = "Role byly úspěšně odstraněny.",
                [Language.English] = "Roles have been successfully deleted."
            },
            [_textsKeys.CouldNotDelete] =
            {
                [Language.Czech] = "Zaměstnanecké role se nepodařilo odebrat. Prosím, kontaktujte moderátory.",
                [Language.English] = "Zaměstnanecké role se nepodařilo odebrat. Prosím, kontaktujte moderátory."
            },
            [_textsKeys.NotVerifiedYet] =
            {
                [Language.Czech] = "Ahoj, ještě nejsi ověřený!\n" +
                                   "1) Pro ověření a přidělení rolí dle UserMap klikni na tlačítko dole. ✅\n" +
                                   "2) Následně znovu klikni na tlačítko pro přidání zaměstnaneckých rolí. 👑",
                [Language.English] = "Hi, you are not verified yet!\n" +
                                     "1) Click the button below to verify and assign roles according to UserMap. ✅\n" +
                                     "2) Then click the button to add employee roles again. 👑"
            },
            [_textsKeys.AlreadyVerified] =
            {
                [Language.Czech] = "Ahoj, už jsi ověřený.\n" +
                                   "Pro aktualizaci zaměstnaneckých rolí klikni na tlačítko.",
                [Language.English] = "Hi, you are already verified.\n" +
                                     "Click the button to update employee roles."
            },
            [_textsKeys.VerifyBtn] = { [Language.Czech] = "Ověřit se", [Language.English] = "Verify" },
            [_textsKeys.UpdateStaffRolesBtn] =
            {
                [Language.Czech] = "Aktualizovat role zaměstnance", [Language.English] = "Update staff roles"
            },
            [_textsKeys.VerifyStaffRolesBtn] =
            {
                [Language.Czech] = "Ověřit role zaměstnance", [Language.English] = "Verify staff roles"
            },
            [_textsKeys.RemoveRolesBtn] = { [Language.Czech] = "Odebrat role", [Language.English] = "Remove roles" },
            [_textsKeys.VerifyStaff] =
            {
                [Language.Czech] = "Ahoj, pro ověření rolí zaměstnance klikni na tlačítko.",
                [Language.English] = "Hi, click the button to verify the staff roles."
            },
        };

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

            Language currentLanguage = Language.English;
            if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
            {
                currentLanguage = Language.Czech;
            }

            DiscordUser user = eventArgs.User;
            DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
            var builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);

            // Check if the button to remove staff roles was pressed.
            if (eventArgs.Id == _buttonOptions.StaffRemoveRoleId)
            {
                bool revoked = await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
                builder.Content = _texts[_textsKeys.SuccessFullyDeleted][currentLanguage];

                if (!revoked)
                {
                    _logger.LogInformation(
                        "Ungranting roles for user {Username} (id {Id}) failed",
                        eventArgs.User.Username,
                        eventArgs.User.Id
                    );
                    builder.Content = _texts[_textsKeys.CouldNotDelete][currentLanguage];
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
                builder.Content = _texts[_textsKeys.NotVerifiedYet][currentLanguage];
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        verificationLink,
                        _texts[_textsKeys.VerifyBtn][currentLanguage],
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
                builder.Content = _texts[_textsKeys.AlreadyVerified][currentLanguage];
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        _texts[_textsKeys.UpdateStaffRolesBtn][currentLanguage],
                        false,
                        new DiscordComponentEmoji("👑")
                    ),
                    new DiscordButtonComponent(
                        ButtonStyle.Danger,
                        _buttonOptions.StaffRemoveRoleId,
                        _texts[_textsKeys.RemoveRolesBtn][currentLanguage],
                        false,
                        new DiscordComponentEmoji("🗑️")
                    )
                );
            }
            else
            {
                builder.Content = _texts[_textsKeys.VerifyStaff][currentLanguage];
                builder.AddComponents(new DiscordLinkButtonComponent(
                    link,
                    _texts[_textsKeys.VerifyStaffRolesBtn][currentLanguage],
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
