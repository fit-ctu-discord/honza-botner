using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class ButtonManager : IButtonManager
    {
        private readonly CommonCommandOptions _config;
        private readonly DiscordConfig _dcConfig;

        public ButtonManager(IOptions<CommonCommandOptions> options, IOptions<DiscordConfig> config)
        {
            _config = options.Value;
            _dcConfig = config.Value;

        }

        public async Task SetupButtons(GuildDownloadCompletedEventArgs eventArgs)
        {
            if (eventArgs.Guilds == null) return;

            DiscordGuild guild;

            try
            {
                guild = eventArgs.Guilds[_dcConfig.GuildId ?? 0];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            DiscordChannel channel = guild.GetChannel(_config.VerificationChannelId);
            if (channel.Type != ChannelType.Text) return;

            DiscordMessage message = await channel.GetMessageAsync(_config.VerificationMessageId);

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Content)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "user-verification", "Ověř se!", false, new DiscordComponentEmoji("⚡")),
                    new DiscordButtonComponent(ButtonStyle.Primary, "staff-verification", "Získat role zaměstnance", false, new DiscordComponentEmoji("👑"))
                });

            await message.ModifyAsync(builder);
        }

    }
}
