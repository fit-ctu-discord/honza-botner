using System.Linq;
using System.Threading.Tasks;
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
    public class StaffVerificationEventHandler : IEventHandler<MessageReactionAddEventArgs>,
        IEventHandler<MessageReactionRemoveEventArgs>
    {
        private readonly IUrlProvider _urlProvider;
        private readonly CommonCommandOptions _config;
        private readonly DiscordRoleConfig _discordRoleConfig;
        private readonly IDiscordRoleManager _roleManager;
        private readonly ILogger<StaffVerificationEventHandler> _logger;

        public StaffVerificationEventHandler(IUrlProvider urlProvider,
            IOptions<CommonCommandOptions> options,
            IOptions<DiscordRoleConfig> discordRoleConfig,
            IDiscordRoleManager roleManager,
            ILogger<StaffVerificationEventHandler> logger)
        {
            _urlProvider = urlProvider;
            _config = options.Value;
            _discordRoleConfig = discordRoleConfig.Value;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionAddEventArgs eventArgs)
        {
            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId))
                return EventHandlerResult.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.StaffVerificationEmojiName)) return EventHandlerResult.Continue;

            DiscordUser user = eventArgs.User;
            DiscordMember member = eventArgs.Guild.Members[user.Id];
            DiscordDmChannel channel = await member.CreateDmChannelAsync();

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
                await channel.SendMessageAsync(
                    $"Ahoj, ještě nejsi ověřený!\n" +
                    $"1) Pro ověření ✅ a přidělení rolí dle UserMap klikni na odkaz: {verificationLink}\n" +
                    "2) Následně znovu klikni na tlačítko 👑 pro přidání zaměstnaneckých rolí.");
                return EventHandlerResult.Stop;
            }

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Staff);
            await channel.SendMessageAsync($"Ahoj, pro získání rolí zaměstnance klikni na: {link}");

            return EventHandlerResult.Stop;
        }

        public async Task<EventHandlerResult> Handle(MessageReactionRemoveEventArgs eventArgs)
        {
            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId))
                return EventHandlerResult.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.StaffVerificationEmojiName)) return EventHandlerResult.Continue;

            bool revoked = await _roleManager.RevokeRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
            if (!revoked)
            {
                _logger.LogWarning("Ungranting roles for user {Username} (id {Id}) failed", eventArgs.User.Username,
                    eventArgs.User.Id);
                DiscordDmChannel channel = await eventArgs.Guild.Members[eventArgs.User.Id].CreateDmChannelAsync();
                await channel.SendMessageAsync("Staff role se nepodařilo odebrat. Prosím, kontaktujte moderátory.");
            }

            return EventHandlerResult.Continue;
        }
    }
}
