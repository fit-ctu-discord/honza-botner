using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Commands
{
    [Group("voice")]
    [Description("Commands to control (and only works in) custom voice channels.")]
    public class VoiceCommands : BaseCommandModule
    {
        private readonly IVoiceManager _voiceManager;
        private readonly CustomVoiceOptions _voiceConfig;
        private readonly ILogger<VoiceCommands> _logger;

        public VoiceCommands(IVoiceManager voiceManager, IOptions<CustomVoiceOptions> options,
            ILogger<VoiceCommands> logger)
        {
            _voiceManager = voiceManager;
            _voiceConfig = options.Value;
            _logger = logger;
        }

        [Command("add")]
        [Aliases("new")]
        [Description("Create new voice channel. Users has 30 seconds to join.")]
        [Priority(2)]
        public async Task AddVoiceChannelWithLimit(
            CommandContext ctx,
            [Description("Name of the channel.")] string name,
            [Description("Limit number of members who can join.")]
            int limit = 0
        )
        {
            await AddVoiceAsync(ctx, name, limit);
        }

        [Command("add")]
        [Priority(1)]
        public async Task AddVoiceChannel(
            CommandContext ctx,
            [RemainingText, Description("Name of the channel.")]
            string name
        )
        {
            await AddVoiceAsync(ctx, name);
        }

        [Command("edit")]
        [Aliases("rename")]
        [Description("Edits the name (and limit) of the voice channel you are connected to.")]
        [Priority(2)]
        public async Task EditVoiceChannelWithLimit(
            CommandContext ctx,
            [Description("New name of the channel.")]
            string newName,
            [Description("Limit number of members who can join.")]
            int? limit = null
        )
        {
            await EditVoiceAsync(ctx, newName, limit);
        }

        [Command("edit")]
        [Priority(1)]
        public async Task EditVoiceChannel(
            CommandContext ctx,
            [RemainingText, Description("New name of the channel.")]
            string newName
        )
        {
            await EditVoiceAsync(ctx, newName);
        }

        private bool InValidChannel(DiscordChannel channel)
        {
            return _voiceConfig.CommandChannelsIds.Contains(channel.Id);
        }

        private async Task AddVoiceAsync(
            CommandContext ctx,
            string name,
            int limit = 0
        )
        {
            try
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
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't add a voice channel");
            }
        }

        private async Task EditVoiceAsync(
            CommandContext ctx,
            string newName,
            int? limit = null
        )
        {
            try
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
            catch (Exception e)
            {
                _logger.LogWarning(e, "Couldn't edit a voice channel");
            }
        }
    }
}
