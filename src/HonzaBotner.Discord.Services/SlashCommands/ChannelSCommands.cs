using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("channel", "Prikazy spravujici kanaly")]
    public class ChannelSCommands : ApplicationCommandModule
    {
        private readonly ILogger<ChannelSCommands> _logger;

        public ChannelSCommands(ILogger<ChannelSCommands> logger)
        {
            _logger = logger;
        }

        [SlashCommand("clone", "Duplikuje kanal", false)]
        [SlashRequirePermissions(Permissions.ManageChannels, false)]
        [SlashRequireGuild]
        public async Task CloneAsync(InteractionContext ctx,
            [Option("kanal", "Kanal co se zduplikuje")] DiscordChannel channel,
            [Option("jmeno", "Jmeno noveho kanalu")] string name)
        {
            DiscordChannel cloned = await channel.CloneAsync();

            await cloned.ModifyAsync(model =>
            {
                model.Name = name;
                model.Position = channel.Position;
            });

            _logger.LogInformation("Duplicated channel `{Name}` by `{MemberName}`",
                channel.Name, ctx.Member.DisplayName);
            var response = new DiscordInteractionResponseBuilder().WithContent($"Duplikovano jako {cloned.Mention}");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

    }
}
