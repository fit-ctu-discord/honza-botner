using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class StaffVerificationReactionHandler : IReactionHandler
    {
        private readonly IUrlProvider _urlProvider;
        private readonly CommonCommandOptions _config;
        private readonly IDiscordRoleManager _roleManager;
        private readonly ILogger<StaffVerificationReactionHandler> _logger;

        public StaffVerificationReactionHandler(IUrlProvider urlProvider,
            IOptions<CommonCommandOptions> options,
            IDiscordRoleManager roleManager,
            ILogger<StaffVerificationReactionHandler> logger)
        {
            _urlProvider = urlProvider;
            _config = options.Value;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            // https://discordapp.com/channels/366970031445377024/507515506073403402/686745124885364770

            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId))
                return IReactionHandler.Result.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.StaffVerificationEmojiName)) return IReactionHandler.Result.Continue;

            DiscordUser user = eventArgs.User;
            DiscordDmChannel channel = await eventArgs.Guild.Members[user.Id].CreateDmChannelAsync();

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Staff);
            await channel.SendMessageAsync($"Ahoj, pro získání rolí zaměstnance klikni na: {link}");

            return IReactionHandler.Result.Stop;
        }

        public async Task<IReactionHandler.Result> HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs)
        {
            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId))
                return IReactionHandler.Result.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.StaffVerificationEmojiName)) return IReactionHandler.Result.Continue;

            bool ungranted = await _roleManager.UngrantRolesPoolAsync(eventArgs.User.Id, RolesPool.Staff);
            if (!ungranted)
            {
                _logger.LogWarning("Ungranting roles failed.", eventArgs.User.Username, eventArgs.User.Id);
            }

            return IReactionHandler.Result.Continue;
        }
    }
}
