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
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class VerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly ButtonOptions _buttonOptions;
        private DiscordRoleConfig _discordRoleConfig;

        private enum _textKeys
        {
            AlreadyVerified,
            Verify,
            VerifyBtn,
            UpdateRolesBtn
        }

        private readonly Dictionary<_textKeys, Dictionary<Language, string>> _texts = new()
        {
            [_textKeys.AlreadyVerified] =
            {
                [Language.Czech] = "Ahoj, už jsi ověřený.\nPro aktualizaci rolí klikni na tlačítko.",
                [Language.English] = "Hi, you are already verified.\nClick the button to update the roles."
            },
            [_textKeys.Verify] =
            {
                [Language.Czech] = "Ahoj, pro ověření a přidělení rolí klikni na tlačítko.",
                [Language.English] = "Hi, click the button to verify and assign roles."
            },
            [_textKeys.VerifyBtn] = { [Language.Czech] = "Ověřit se", [Language.English] = "Verify" },
            [_textKeys.UpdateRolesBtn] =
            {
                [Language.Czech] = "Aktualizovat role", [Language.English] = "Update roles"
            }
        };

        public VerificationEventHandler(
            IUrlProvider urlProvider,
            IOptions<ButtonOptions> options,
            IOptions<DiscordRoleConfig> discordRoleConfig
        )
        {
            _urlProvider = urlProvider;
            _buttonOptions = options.Value;
            _discordRoleConfig = discordRoleConfig.Value;
        }

        public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Id != _buttonOptions.VerificationId) return EventHandlerResult.Continue;

            var currentLanguage = Language.English;
            if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
            {
                currentLanguage = Language.Czech;
            }

            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);

            DiscordUser user = eventArgs.User;
            DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);

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

            if (isAuthenticated)
            {
                builder.Content = _texts[_textKeys.AlreadyVerified][currentLanguage];
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        _texts[_textKeys.UpdateRolesBtn][currentLanguage],
                        false,
                        new DiscordComponentEmoji("🔄")
                    )
                );
            }
            else
            {
                builder.Content = _texts[_textKeys.Verify][currentLanguage];
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        _texts[_textKeys.VerifyBtn][currentLanguage],
                        false,
                        new DiscordComponentEmoji("✅")
                    )
                );
            }

            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);

            return EventHandlerResult.Stop;
        }
    }
}
