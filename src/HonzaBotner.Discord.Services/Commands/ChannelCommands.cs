using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Attributes;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("channel")]
    [Description("Commands to interact with channels.")]
    [Hidden]
    [RequireMod]
    public class ChannelCommands : BaseCommandModule
    {
        private readonly ILogger<ChannelCommands> _logger;

        public ChannelCommands(ILogger<ChannelCommands> logger)
        {
            _logger = logger;
        }

        [Command("clone")]
        [Description("Clones specified channel.")]
        public async Task SendMessage(CommandContext ctx,
            [Description("Channel to clone.")] DiscordChannel channel,
            [Description("Name of the cloned channel.")]
            string name,
            [Description("Additional roles to view this channel")]
            params DiscordRole[] roles)
        {
            DiscordChannel? cloned = null;
            try
            {
                cloned = await channel.CloneAsync();
            }
            catch
            {
                _logger.LogError("Failed to clone channel {0} with name {1}.", channel, name);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }

            await cloned!.ModifyAsync(model =>
            {
                model.Name = name;
                model.Position = channel.Position;
            });

            foreach (DiscordRole role in roles)
            {
                try
                {
                    await cloned.AddOverwriteAsync(role, Permissions.AccessChannels);
                }
                catch
                {
                    _logger.LogWarning("Failed to grant access to role {0} in channel {1}.", role, channel);
                }
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }
    }
}
