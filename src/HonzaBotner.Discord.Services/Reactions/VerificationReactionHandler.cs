using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class VerificationReactionHandler : IReactionHandler
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUrlProvider _urlProvider;
        private readonly CommonCommandOptions _config;

        public VerificationReactionHandler(IAuthorizationService authorizationService, IUrlProvider urlProvider, IOptions<CommonCommandOptions> options)
        {
            _authorizationService = authorizationService;
            _urlProvider = urlProvider;
            _config = options.Value;
        }

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            // https://discordapp.com/channels/366970031445377024/507515506073403402/686745124885364770

            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId)) return IReactionHandler.Result.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.VerificationEmojiName)) return IReactionHandler.Result.Continue;

            DiscordUser user = eventArgs.User;
            DiscordDmChannel channel = await eventArgs.Guild.Members[user.Id].CreateDmChannelAsync();

            if (await _authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync("Již jsi autorizován");
            }
            else
            {
                string link = _urlProvider.GetAuthLink(user.Id);
                await channel.SendMessageAsync($"Ahoj, autorizuj se prosím pomocí tohoto odkazu: {link}");
            }

            return IReactionHandler.Result.Stop;
        }
    }
}
