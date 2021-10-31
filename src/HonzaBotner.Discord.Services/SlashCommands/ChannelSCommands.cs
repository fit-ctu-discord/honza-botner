using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.SlashCommands
{
    [SlashCommandGroup("channel", "Commands to manage channels")]
    public class ChannelSCommands : ApplicationCommandModule
    {
        private readonly ILogger<ChannelSCommands> _logger;

        public ChannelSCommands(ILogger<ChannelSCommands> logger)
        {
            _logger = logger;
        }

        [SlashCommand("clone", "Clones the channel")]
        [SlashRequireGuild]
        public async Task CloneAsync(InteractionContext ctx,
            [Option("oldChannel", "Channel we want to duplicate")] DiscordChannel channel,
            [Option("newChannel", "New channel's name")] string name)
        {
            DiscordChannel cloned = await channel.CloneAsync();

            await cloned.ModifyAsync(model =>
            {
                model.Name = name;
                model.Position = channel.Position;
            });

            _logger.LogInformation("Duplicated channel `{Name}` by `{MemberName}`",
                channel.Name, ctx.Member.DisplayName);
            var response = new DiscordInteractionResponseBuilder().WithContent($"Created channel {cloned.Mention}");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

    }
}
