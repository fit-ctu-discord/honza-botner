using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class StaffVerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly DiscordRoleConfig _discordRoleConfig;
        private readonly IDiscordRoleManager _roleManager;
        private readonly ILogger<StaffVerificationEventHandler> _logger;

        public StaffVerificationEventHandler(IUrlProvider urlProvider,
            IOptions<DiscordRoleConfig> discordRoleConfig,
            IDiscordRoleManager roleManager,
            ILogger<StaffVerificationEventHandler> logger)
        {
            _urlProvider = urlProvider;
            _discordRoleConfig = discordRoleConfig.Value;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Id != "staff-verification") return EventHandlerResult.Continue;

            DiscordUser user = eventArgs.User;
            DiscordMember member = eventArgs.Guild.Members[user.Id];
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);

            bool isAuthenticated = false;
            foreach (ulong roleId in _discordRoleConfig.AuthenticatedRoleIds)
            {
                if (member.Roles.Select(role => role.Id).Contains(roleId))
                {
                    isAuthenticated = true;
                    break;
                }
            }

            // Check if the user is authenticated first.
            if (!isAuthenticated)
            {
                string verificationLink = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);
                builder.Content =
                    "Ahoj, ještě nejsi ověřený!\n" +
                    "1) Pro ověření ✅ a přidělení rolí dle UserMap klikni na tlačítko dole\n" +
                    "2) Následně znovu klikni na tlačítko pro přidání 👑 zaměstnaneckých rolí.";
                builder.AddComponents(new DiscordLinkButtonComponent(verificationLink, "Ověřit se!", false,
                    new DiscordComponentEmoji("✅")));
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    builder);

                return EventHandlerResult.Stop;
            }

            await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Staff);
            builder.Content = "Ahoj, klikni pro ověření zaměstnaneckých rolí";
            builder.AddComponents(new DiscordLinkButtonComponent(link, "Jsem zaměstnanec!", false,
                new DiscordComponentEmoji("👑")));
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);

            return EventHandlerResult.Stop;
        }
    }
}
