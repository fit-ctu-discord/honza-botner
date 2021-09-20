using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers
{
    public class ButtonManager : IButtonManager
    {
        private readonly ButtonOptions _buttonOptions;

        public ButtonManager(IOptions<ButtonOptions> buttonConfig)
        {
            _buttonOptions = buttonConfig.Value;
        }

        public async Task SetupVerificationButtons(DiscordMessage target)
        {
            DiscordMessage message = target;

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
