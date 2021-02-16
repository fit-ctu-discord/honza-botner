﻿using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Reactions
{
    public class VerificationReactionHandler : IReactionHandler
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUrlProvider _urlProvider;
        private readonly CommonCommandOptions _config;

        public VerificationReactionHandler(IAuthorizationService authorizationService, IUrlProvider urlProvider,
            IOptions<CommonCommandOptions> options)
        {
            _authorizationService = authorizationService;
            _urlProvider = urlProvider;
            _config = options.Value;
        }

        public async Task<IReactionHandler.Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs)
        {
            // https://discordapp.com/channels/366970031445377024/507515506073403402/686745124885364770

            if (!(eventArgs.Message.Id == _config.VerificationMessageId
                  && eventArgs.Message.ChannelId == _config.VerificationChannelId))
                return IReactionHandler.Result.Continue;
            if (!eventArgs.Emoji.Name.Equals(_config.VerificationEmojiName)) return IReactionHandler.Result.Continue;

            DiscordUser user = eventArgs.User;
            DiscordDmChannel channel = await eventArgs.Guild.Members[user.Id].CreateDmChannelAsync();

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);

            if (await _authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync($"Ahoj, už jsi ověřený.\nPro aktualizaci rolí dle UserMap klikni na odkaz: {link}");
            }
            else
            {
                await channel.SendMessageAsync($"Ahoj, pro ověření a přidělení rolí dle UserMap klikni na odkaz: {link}");
            }

            return IReactionHandler.Result.Stop;
        }
    }
}
