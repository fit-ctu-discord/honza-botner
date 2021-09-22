using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers
{
    public class ButtonManager : IButtonManager
    {
        private readonly ButtonOptions _buttonOptions;
        private readonly ILogger<ButtonManager> _logger;

        public ButtonManager(IOptions<ButtonOptions> buttonConfig, ILogger<ButtonManager> logger)
        {
            _buttonOptions = buttonConfig.Value;
            _logger = logger;
        }

        public async Task SetupVerificationButtons(DiscordMessage target)
        {
            DiscordMessage message = target;

            if (_buttonOptions.VerificationId is null || _buttonOptions.StaffVerificationId is null)
            {
                _logger.LogWarning("'VerificationId' or 'StaffVerificationId' not set in config");
                return;
            }

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Content)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, _buttonOptions.VerificationId, "Ověř se!",
                        false, new DiscordComponentEmoji("✅")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, _buttonOptions.StaffVerificationId,
                        "Přidat role zaměstnance", false, new DiscordComponentEmoji("👑"))
                });

            await message.ModifyAsync(builder);
        }

        public async Task RemoveButtonsFromMessage(DiscordMessage target)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithContent(target.Content);
            await target.ModifyAsync(builder);
        }

    }
}
