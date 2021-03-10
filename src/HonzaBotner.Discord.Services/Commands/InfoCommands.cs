using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("info")]
    [Description("Commands to get you info about Discord bot and other things.")]
    [RequireMod]
    public class InfoCommands : BaseCommandModule
    {
        private readonly ILogger<InfoCommands> _logger;
        private readonly InfoOptions _infoOptions;

        public InfoCommands(ILogger<InfoCommands> logger, IOptions<InfoOptions> infoOptions)
        {
            _logger = logger;
            _infoOptions = infoOptions.Value;
        }

        [GroupCommand]
        [Description("Clones specified channel.")]
        public async Task BotInfo(CommandContext ctx)
        {
            StringBuilder stringBuilder = new();
            stringBuilder
                .Append("Tohoto bota vyvíjí komunita. " +
                        "Budeme rádi, pokud se k vývoji přidáš a pomůžeš nám bota dále vylepšovat.")
                .Append("\n\n")
                .Append($"Zdrojový kód najdeš [zde]({_infoOptions.RepositoryUrl}).")
                .Append("\n")
                .Append($"Hlásit chyby můžeš [tady]({_infoOptions.IssueTrackerUrl}).")
                ;

            try
            {
                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Client.CurrentUser.Username, IconUrl = ctx.Client.CurrentUser.AvatarUrl
                    },
                    Title = "Informace o botovi",
                    Description = stringBuilder.ToString()
                };

                await ctx.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't build and send a embed message");
            }
        }
    }
}
