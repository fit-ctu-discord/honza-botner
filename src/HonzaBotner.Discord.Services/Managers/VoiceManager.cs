using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers;

public class VoiceManager : IVoiceManager
{
    private readonly IGuildProvider _guildProvider;
    private readonly CustomVoiceOptions _voiceConfig;
    private readonly ILogger<VoiceManager> _logger;

    public VoiceManager(IGuildProvider guildProvider, IOptions<CustomVoiceOptions> options,
        ILogger<VoiceManager> logger)
    {
        _guildProvider = guildProvider;
        _voiceConfig = options.Value;
        _logger = logger;
    }

    public async Task AddNewVoiceChannelAsync(
        DiscordChannel channelToCloneFrom, DiscordMember member,
        string? name, long? limit, bool? isPublic)
    {
        name = ConvertStringToValidState(name);

        try
        {
            string? userName = ConvertStringToValidState(member.Nickname, member.Username);
            DiscordChannel newChannel =
                await channelToCloneFrom.CloneAsync($"Member {userName} created new voice channel.");

            await EditChannelAsync(false, newChannel, name, limit, isPublic, userName);

            try
            {
                if (member.VoiceState?.Channel != null)
                {
                    await member.PlaceInAsync(newChannel);
                }
            }
            catch
            {
                // User disconnected while we were placing them.
            }

            // Placing the member in the channel failed, so remove it after some time.
            Task _ = Task.Run(async () =>
            {
                await Task.Delay(1000 * _voiceConfig.RemoveAfterCommandInSeconds);
                await DeleteUnusedVoiceChannelAsync(newChannel);
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Creating voice channel failed");
        }
    }

    public async Task<bool> EditVoiceChannelAsync(DiscordMember member, string? newName = null, long? limit = null,
        bool? isPublic = null)
    {
        if (member.VoiceState?.Channel == null || member.VoiceState?.Channel.Id == _voiceConfig.ClickChannelId)
        {
            return false;
        }

        newName = ConvertStringToValidState(newName);

        DiscordChannel customVoiceCategory = member.Guild.GetChannel(_voiceConfig.ClickChannelId).Parent;

        if (!customVoiceCategory.Equals(member.VoiceState?.Channel?.Parent))
        {
            return false;
        }

        try
        {
            string? userName = ConvertStringToValidState(member.Nickname, member.Username);
            await EditChannelAsync(true, member.VoiceState?.Channel, newName, limit, isPublic, userName);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Editing voice channel failed");
        }

        return false;
    }

    public async Task DeleteUnusedVoiceChannelAsync(DiscordChannel channel)
    {
        if (channel.Id == _voiceConfig.ClickChannelId) return;

        if (!channel.Parent.Equals(channel.Guild.GetChannel(_voiceConfig.ClickChannelId).Parent)) return;

        if (!channel.Users.Any())
        {
            try
            {
                await channel.DeleteAsync();
            }
            catch
            {
                // ignored
            }
        }
    }

    public async Task DeleteAllUnusedVoiceChannelsAsync()
    {
        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        DiscordChannel customVoiceCategory = guild.GetChannel(_voiceConfig.ClickChannelId).Parent;
        foreach (DiscordChannel discordChannel in customVoiceCategory.Children)
        {
            await DeleteUnusedVoiceChannelAsync(discordChannel);
        }
    }

    private string? ConvertStringToValidState(string? input, string? defaultValue = null)
    {
        input = Regex.Replace(input ?? "", @"\p{C}+", string.Empty);
        return input.Trim().Length == 0 ? defaultValue : input.Substring(0, Math.Min(input.Length, 30));
    }

    private async Task EditChannelAsync(bool isEdit, DiscordChannel? channel, string? name, long? limit,
        bool? isPublic,
        string? userName)
    {
        if (channel == null) return;

        await channel.ModifyAsync(model =>
        {
            if (isEdit && name == null)
            {
                // You can skip name if editing.
            }
            else
            {
                model.Name = name ?? $"{userName ?? "FIŤÁK"}'s channel";
            }

            if (limit is not null)
            {
                model.Userlimit = (int) Math.Max(Math.Min(limit.Value, 99), 0);
            }
        });

        if (isPublic is not null)
        {
            if (isPublic == true)
            {
                await channel.AddOverwriteAsync(
                    channel.Guild.EveryoneRole,
                    Permissions.AccessChannels
                );
            }
            else
            {
                await channel.AddOverwriteAsync(
                    channel.Guild.EveryoneRole,
                    deny: Permissions.AccessChannels
                );
            }
        }
    }
}
