using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class VerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUrlProvider _urlProvider;

        public VerificationEventHandler(IAuthorizationService authorizationService, IUrlProvider urlProvider)
        {
            _authorizationService = authorizationService;
            _urlProvider = urlProvider;
        }

        public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
        {
            // https://discordapp.com/channels/366970031445377024/507515506073403402/686745124885364770

            if (eventArgs.Id != "user-verification") return EventHandlerResult.Continue;
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            DiscordUser user = eventArgs.User;
            DiscordDmChannel channel = await eventArgs.Guild.Members[user.Id].CreateDmChannelAsync();

            string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);

            if (await _authorizationService.IsUserVerified(user.Id))
            {
                await channel.SendMessageAsync(
                    $"Ahoj, už jsi ověřený.\nPro aktualizaci rolí dle UserMap klikni na odkaz: {link}");
            }
            else
            {
                await channel.SendMessageAsync(
                    $"Ahoj, pro ověření a přidělení rolí dle UserMap klikni na odkaz: {link}");
            }

            return EventHandlerResult.Stop;
        }
    }
}
