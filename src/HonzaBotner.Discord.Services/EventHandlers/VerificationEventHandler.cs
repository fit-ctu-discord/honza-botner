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
                builder.Content = "Ahoj, už jsi ověřený.\n" +
                                  "Pro aktualizaci rolí klikni na tlačítko.";
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        "Aktualizovat role",
                        false,
                        new DiscordComponentEmoji("🔄")
                    )
                );
            }
            else
            {
                builder.Content = "Ahoj, pro ověření a přidělení rolí klikni na tlačítko.";
                builder.AddComponents(
                    new DiscordLinkButtonComponent(
                        link,
                        "Ověřit se",
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
