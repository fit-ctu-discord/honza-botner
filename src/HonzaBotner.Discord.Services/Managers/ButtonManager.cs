using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Discord.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers
{
    public class ButtonManager : IButtonManager
    {
        private readonly ButtonOptions _buttonOptions;
        private readonly ILogger<ButtonManager> _logger;
        private readonly ITranslation _translation;

        public ButtonManager(
            IOptions<ButtonOptions> buttonConfig,
            ILogger<ButtonManager> logger,
            ITranslation translation)
        {
            _buttonOptions = buttonConfig.Value;
            _logger = logger;
            _translation = translation;
        }

        public async Task SetupVerificationButtons(DiscordMessage message)
        {
            if (_buttonOptions.VerificationId is null || _buttonOptions.StaffVerificationId is null)
            {
                _logger.LogWarning("'VerificationId' or 'StaffVerificationId' not set in config");
                return;
            }

            if (_buttonOptions.CzechChannelsIds?.Contains(message.ChannelId) ?? false)
            {
                _translation.SetLanguage(ITranslation.Language.Czech);
            }

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Content)
                .AddComponents(
                    new DiscordButtonComponent(
                        ButtonStyle.Primary,
                        _buttonOptions.VerificationId,
                        _translation["VerifyRolesButton"],
                        false,
                        new DiscordComponentEmoji("✅")
                    ),
                    new DiscordButtonComponent(
                        ButtonStyle.Secondary,
                        _buttonOptions.StaffVerificationId,
                        _translation["VerifyStaffRolesButton"],
                        false,
                        new DiscordComponentEmoji("👑")
                    )
                );

            await message.ModifyAsync(builder);
        }

        public async Task RemoveButtonsFromMessage(DiscordMessage target)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithContent(target.Content);
            await target.ModifyAsync(builder);
        }
    }
}
