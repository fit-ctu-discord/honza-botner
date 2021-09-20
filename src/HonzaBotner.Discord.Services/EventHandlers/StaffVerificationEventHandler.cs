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

            DiscordUser user = eventArgs.User;
            DiscordMember member = eventArgs.Guild.Members[user.Id];
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);

            // First check if the button to remove staff roles was pressed
            if (eventArgs.Id == _buttonOptions.StaffRemoveRoleId)
            {
                bool revoked = await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
                builder.Content = "Role úspěšně odstraněny";
                if (!revoked)
                {
                    _logger.LogWarning("Ungranting roles for user {Username} (id {Id}) failed", eventArgs.User.Username,
                        eventArgs.User.Id);
                    builder.Content = "Staff role se nepodařilo odebrat. Prosím, kontaktujte moderátory.";
                }

                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
                return EventHandlerResult.Stop;
            }

            // Second check if the user is authenticated
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

            // Third check if the user already has some staff roles
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

            if (isStaffAuthenticated)
            {
                builder.Content = "Ahoj, už jsi ověřený,\nChceš aktualizovat svoje role přes UserMap?";
                builder.AddComponents(new DiscordComponent[]
                {
                    new DiscordLinkButtonComponent(link, "Aktualizovat role zaměstnance!",
                        false, new DiscordComponentEmoji("👑")),
                    new DiscordButtonComponent(ButtonStyle.Danger, _buttonOptions.StaffRemoveRoleId, "Odebrat role",
                        false, new DiscordComponentEmoji("💣"))
                });
            }
            else
            {
                builder.Content = "Ahoj, klikni na tlačítko pro ověřená rolí zaměstnance!";
                builder.AddComponents(new DiscordLinkButtonComponent(link, "Ověřit role zaměstnance!",
                    false, new DiscordComponentEmoji("👑")));
            }
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                builder);

            return EventHandlerResult.Stop;
        }
    }
}
