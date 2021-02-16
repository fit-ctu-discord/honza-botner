using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services
{
    public class VoiceManager : IVoiceManager
    {
        private readonly IGuildProvider _guildProvider;
        private readonly DiscordWrapper _discordWrapper;
        private readonly CommonCommandOptions _config;

        private DiscordClient Client => _discordWrapper.Client;

        public VoiceManager(IGuildProvider guildProvider, DiscordWrapper discordWrapper,
            IOptions<CommonCommandOptions> options)
        {
            _guildProvider = guildProvider;
            _discordWrapper = discordWrapper;
            _config = options.Value;
        }

        public async Task Init()
        {
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            // Startup cleaning.
            await DeleteAllUnusedVoiceChannelsAsync();
        }

        private async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            if (args.After.Channel?.Id == _config.CustomVoiceClickChannel)
            {
                await AddNewVoiceChannelAsync(args.Channel, await args.Guild.GetMemberAsync(args.User.Id));
            }

            if (args.Before.Channel != null)
            {
                if (args.Before.Channel.Parent.Id == _config.CustomVoiceCategory &&
                    args.Before.Channel.Id != _config.CustomVoiceClickChannel)
                {
                    await DeleteUnusedVoiceChannelAsync(args.Before.Channel);
                }
            }
        }

        public async Task AddNewVoiceChannelAsync(
            DiscordChannel channelToCloneFrom, DiscordMember member,
            string? name = null, int? limit = 0)
        {
            if (name?.Trim().Length == 0)
            {
                name = null;
            }

            try
            {
                DiscordChannel newChannel =
                    await channelToCloneFrom.CloneAsync($"Member {member.Username} created new voice channel.");
                await newChannel.ModifyAsync(model =>
                {
                    model.Name = name ?? $"{member.Username}'s channel";
                    model.Userlimit = limit;
                });
                await newChannel.AddOverwriteAsync(member, Permissions.ManageChannels | Permissions.MuteMembers);

                try
                {
                    await member.PlaceInAsync(newChannel);
                }
                catch
                {
                    // Placing the member in the channel failed, so remove it after some time.
                    var _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000 * _config.CustomVoiceRemoveAfterCommand);
                        await DeleteUnusedVoiceChannelAsync(newChannel);
                    });
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task<bool> EditVoiceChannelAsync(DiscordMember member, string? newName = null, int? limit = 0)
        {
            if (member.VoiceState.Channel == null || member.VoiceState.Channel.Parent.Id != _config.CustomVoiceCategory)
            {
                return false;
            }

            if ((member.VoiceState.Channel.PermissionsFor(member) & Permissions.ManageChannels) == 0) return false;

            try
            {
                await member.VoiceState.Channel.ModifyAsync(model =>
                {
                    model.Name = newName;

                    if (limit != null)
                    {
                        model.Userlimit = limit;
                    }
                });
                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private async Task DeleteUnusedVoiceChannelAsync(DiscordChannel channel)
        {
            if (channel.Id == _config.CustomVoiceClickChannel) return;

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

        private async Task DeleteAllUnusedVoiceChannelsAsync()
        {
            DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
            foreach (DiscordChannel discordChannel in guild.GetChannel(_config.CustomVoiceCategory).Children)
            {
                await DeleteUnusedVoiceChannelAsync(discordChannel);
            }
        }
    }
}
