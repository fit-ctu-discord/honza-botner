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
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class VerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly ButtonOptions _buttonOptions;
        private DiscordRoleConfig _discordRoleConfig;

        private enum _Language
        {
            CS,
            EN
        }

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

            _Language current = _Language.EN;
            if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
            {
                current = _Language.CS;
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
                builder.Content = current switch {
                    _Language.EN => "Hi, you are already verified.\n" +
                                    "Click the button to update the roles.",
                    _Language.CS => "Ahoj, už jsi ověřený.\n" +
                                    "Pro aktualizaci rolí klikni na tlačítko.",
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        current switch {
                            _Language.EN => "Update roles",
                            _Language.CS => "Aktualizovat role",
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        false,
                        new DiscordComponentEmoji("🔄")
                    )
                );
            }
            else
            {
                builder.Content = current switch {
                    _Language.EN => "Hi, click the button to verify and assign roles.",
                    _Language.CS => "Ahoj, pro ověření a přidělení rolí klikni na tlačítko.",
                    _ => throw new ArgumentOutOfRangeException()
                };
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        current switch {
                            _Language.EN => "Verify",
                            _Language.CS => "Ověřit se",
                            _ => throw new ArgumentOutOfRangeException()
                        },
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
