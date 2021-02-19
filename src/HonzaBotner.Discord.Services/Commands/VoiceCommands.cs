using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("voice")]
    [Description("Commands to control (and only works in) custom voice channels.")]
    public class VoiceCommands : BaseCommandModule
    {
        private readonly IVoiceManager _voiceManager;
        private readonly CustomVoiceOptions _voiceConfig;

        public VoiceCommands(IVoiceManager voiceManager, IOptions<CustomVoiceOptions> options)
        {
            _voiceManager = voiceManager;
            _voiceConfig = options.Value;
        }

        [Command("add")]
        [Aliases("new")]
        [Description("Create new voice channel. Users has 30 seconds to join.")]
        public async Task AddVoiceChannel(
            CommandContext ctx,
            [Description("Name of the channel.")] string name,
            [Description("Limit number of members who can join.")]
            int limit = 0
        )
        {
            await ctx.TriggerTypingAsync();
            if (!InValidChannel(ctx.Channel))
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                return;
            }

            await _voiceManager.AddNewVoiceChannelAsync(ctx.Guild.GetChannel(_voiceConfig.ClickChannelId),
                ctx.Member, name, limit);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
        }

        [Command("edit")]
        [Aliases("rename")]
        [Description("Edits the name (and limit) of the voice channel you are connected to.")]
        public async Task EditVoiceChannel(
            CommandContext ctx,
            [Description("New name of the channel.")]
            string newName,
            [Description("Limit number of members who can join.")]
            int? limit = null
        )
        {
            if (!InValidChannel(ctx.Channel))
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
                return;
            }

            bool success = await _voiceManager.EditVoiceChannelAsync(ctx.Member, newName, limit);

            if (success)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));
            }
            else
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":-1:"));
            }
        }

        private bool InValidChannel(DiscordChannel channel)
        {
            return _voiceConfig.CommandChannelsIds.Contains(channel.Id);
        }
    }
}
