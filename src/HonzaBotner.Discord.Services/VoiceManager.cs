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
        private readonly DiscordWrapper _discordWrapper;
        private readonly CommonCommandOptions _config;

        private DiscordClient Client => _discordWrapper.Client;

        public VoiceManager(DiscordWrapper discordWrapper, IOptions<CommonCommandOptions> options)
        {
            _discordWrapper = discordWrapper;
            _config = options.Value;
        }

        public Task Run()
        {
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            return Task.CompletedTask;
        }

        public async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            if (args.After.Channel?.Id == _config.CustomVoiceClickChannel)
            {
                await AddNewVoiceChannelAsync(client, args.Channel, await args.Guild.GetMemberAsync(args.User.Id));
            }

            if (args.Before.Channel != null)
            {
                if (args.Before.Channel.Parent.Id == _config.CustomVoiceCategory && args.Before.Channel.Id != _config.CustomVoiceClickChannel)
                {
                    await ClearUnusedVoiceChannelAsync(args.Before.Channel);
                }
            }
        }

        public async Task AddNewVoiceChannelAsync(DiscordClient client, DiscordChannel channelToCloneFrom,
            DiscordMember member, string? name = null, int? limit = 0)
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
                    var ignoredTask = Task.Run(async () =>
                    {
                        await Task.Delay(1000 * _config.CustomVoiceRemoveAfterCommand);
                        await ClearUnusedVoiceChannelAsync(newChannel);
                    });
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task ClearUnusedVoiceChannelAsync(DiscordChannel channel)
        {
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
    }
}
