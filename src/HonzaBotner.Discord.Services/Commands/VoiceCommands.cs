using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HonzaBotner.Discord.Services.Attributes;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("voice")]
    [Description("Commands to control custom voice channels.")]
    [RequireChannel(750108543125946448)]
    public class VoiceCommands : BaseCommandModule
    {
        private readonly IVoiceManager _voiceManager;

        public VoiceCommands(IVoiceManager voiceManager) => _voiceManager = voiceManager;

        [Command("add")]
        [Description("Create new voice channel. Users has 30 seconds to join.")]
        public Task AddVoiceChannel(
            CommandContext ctx,
            [Description("Name of the channel.")] string name,
            [Description("Limit number of members who can join.")] int limit = 0
        )
        {
            _voiceManager.AddNewVoiceChannelAsync(ctx.Client, ctx.Guild.GetChannel(810277031089930251), ctx.Member,
                name, limit);
            return ctx.RespondAsync($"I have created new voice channel '{name}' for you!");
        }
    }
}
