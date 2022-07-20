using System;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.SCommands;

[SlashCommandGroup("voice", "Commands to customize own voice channels")]
public class VoiceCommands : ApplicationCommandModule
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

    [SlashCommand("create", "Create new custom voice channel.")]
    public async Task CreateCommandAsync(
        InteractionContext ctx,
        [MaximumLength(100)]
        [Option("name", "Name of the channel.")] string name,
        [Minimum(0), Maximum(99)]
        [Option("limit", "Limit amount of people allowed to join.")] long limit = 0,
        [Option("public", "Should the channel be publicly accessible?")] bool isPublic = false)
    {
        try
        {
            await _voiceManager.AddNewVoiceChannelAsync(ctx.Guild.GetChannel(_voiceConfig.ClickChannelId),
                ctx.Member, name, limit, isPublic);
            await ctx.CreateResponseAsync("Channel created");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Couldn't add a voice channel");
            await ctx.CreateResponseAsync("Channel creation failed", true);
        }
    }

    [SlashCommand("edit", "Edit voice channel you are connected to.")]
    public async Task EditCommandAsync(
        InteractionContext ctx,
        [MaximumLength(100)] [Option("name", "Change name")] string? name = null,
        [Minimum(0), Maximum(99)]
        [Option("limit", "Change limit of people")] long? limit = null,
        [Option("public", "Change whether the channel appears to everyone")] bool? isPublic = null
    )
    {
        if (name is null && limit is null && isPublic is null)
        {
            await ctx.CreateResponseAsync("There is nothing to edit.", true);
            return;
        }

        bool success = false;
        try
        {
            success = await _voiceManager.EditVoiceChannelAsync(ctx.Member, name, limit, isPublic);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Command by {Nickname} to edit voice channel failed", ctx.Member.DisplayName);
        }

        if (success)
        {
            await ctx.CreateResponseAsync("Channel edited successfully");
        }
        else
        {
            await ctx.CreateResponseAsync("Failed while editing channel.", true);
        }
    }
}
