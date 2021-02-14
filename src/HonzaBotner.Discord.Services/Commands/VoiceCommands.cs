using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("voice")]
    [Description("Commands to control custom voice channels.")]
    public class VoiceCommands : BaseCommandModule
    {
        private readonly IVoiceManager _voiceManager;
        private readonly CommonCommandOptions _config;

        public VoiceCommands(IVoiceManager voiceManager, IOptions<CommonCommandOptions> options)
        {
            _voiceManager = voiceManager;
            _config = options.Value;
        }

        [Command("add")]
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

            await _voiceManager.AddNewVoiceChannelAsync(ctx.Guild.GetChannel(_config.CustomVoiceClickChannel),
                ctx.Member,
                name, limit);

            await ctx.RespondAsync($"I have created new voice channel '{name}' for you!");
        }

        [Command("edit")]
        [Description("Create new voice channel. Users has 30 seconds to join.")]
        public async Task EditVoiceChannel(
            CommandContext ctx,
            [Description("Name of the channel.")] string newName,
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
            return _config.CustomVoiceCommandsChannels.Contains(channel.Id);
        }
    }
}
