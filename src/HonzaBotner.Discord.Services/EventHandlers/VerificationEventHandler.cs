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

namespace HonzaBotner.Discord.Services.EventHandlers;

public class VerificationEventHandler : IEventHandler<ComponentInteractionCreateEventArgs>
{
    private readonly IUrlProvider _urlProvider;
    private readonly ButtonOptions _buttonOptions;
    private readonly DiscordRoleConfig _discordRoleConfig;
    private readonly ITranslation _translation;

    public VerificationEventHandler(
        IUrlProvider urlProvider,
        IOptions<ButtonOptions> options,
        IOptions<DiscordRoleConfig> discordRoleConfig,
        ITranslation translation
    )
    {
        _urlProvider = urlProvider;
        _buttonOptions = options.Value;
        _discordRoleConfig = discordRoleConfig.Value;
        _translation = translation;
    }

    public async Task<EventHandlerResult> Handle(ComponentInteractionCreateEventArgs eventArgs)
    {
        if (eventArgs.Id != _buttonOptions.VerificationId) return EventHandlerResult.Continue;

        if (_buttonOptions.CzechChannelsIds?.Contains(eventArgs.Channel.Id) ?? false)
        {
            _translation.SetLanguage(ITranslation.Language.Czech);
        }

        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder().AsEphemeral(true);
        DiscordUser user = eventArgs.User;
        DiscordMember member = await eventArgs.Guild.GetMemberAsync(user.Id);
        string link = _urlProvider.GetAuthLink(user.Id, RolesPool.Auth);

        // Check if the user is authenticated.
        bool isAuthenticated = _discordRoleConfig.AuthenticatedRoleIds.Any(
            roleId => member.Roles.Select(role => role.Id).Contains(roleId)
        );

        if (isAuthenticated)
        {
            builder.Content = _translation["AlreadyVerified"];
            builder.AddComponents(
                new DiscordLinkButtonComponent(
                    link,
                    _translation["UpdateRolesButton"],
                    false,
                    new DiscordComponentEmoji("🔄")
                )
            );
        }
        else
        {
            builder.Content = _translation["Verify"];
            builder.AddComponents(
                new DiscordLinkButtonComponent(
                    link,
                    _translation["VerifyRolesButton"],
                    false,
                    new DiscordComponentEmoji("✅")
                )
            );
        }

        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);

        return EventHandlerResult.Stop;
    }
}
