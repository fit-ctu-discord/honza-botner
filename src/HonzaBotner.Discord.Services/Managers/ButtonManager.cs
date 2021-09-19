using System.Collections.Generic;
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
        private readonly CommonCommandOptions _config;
        private readonly IGuildProvider _guildProvider;

        public ButtonManager(IOptions<CommonCommandOptions> options, IGuildProvider guildProvider)
        {
            _config = options.Value;
            _guildProvider = guildProvider;
        }

        public async Task SetupButtons()
        {
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
            DiscordChannel channel = guild.GetChannel(_config.VerificationChannelId);
            if (channel.Type != ChannelType.Text) return;

            DiscordMessage message = await channel.GetMessageAsync(_config.VerificationMessageId);

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Content)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "user-verification", "Ověř se!", false, new DiscordComponentEmoji("✅")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "staff-verification", "Přidat role zaměstnance", false, new DiscordComponentEmoji("👑"))
                });

            await message.ModifyAsync(builder);
        }

    }
}
