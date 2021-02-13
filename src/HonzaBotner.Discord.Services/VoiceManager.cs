using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Timer = System.Threading.Timer;

namespace HonzaBotner.Discord.Services
{
    public class VoiceManager : IVoiceManager
    {
        private readonly IGuildProvider _guildProvider;
        private readonly DiscordWrapper _discordWrapper;
        private DiscordClient Client => _discordWrapper.Client;

        public VoiceManager(DiscordWrapper discordWrapper, IGuildProvider guildProvider)
        {
            _discordWrapper = discordWrapper;
            _guildProvider = guildProvider;
        }

        public Task Run()
        {
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            return Task.CompletedTask;
        }

        public async Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            // TODO hardcoded IDs, check guild?
            if (args.After.Channel?.Id == 810277031089930251)
            {
                await AddNewVoiceChannelAsync(client, args.Channel, await args.Guild.GetMemberAsync(args.User.Id));
            }

            if (args.Before.Channel != null)
            {
                // TODO: category id, generator channel id
                if (args.Before.Channel.Parent.Id == 750055929340231714 && args.Before.Channel.Id != 810277031089930251)
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

            DiscordChannel newChannel =
                await channelToCloneFrom.CloneAsync($"Member {member.Username} created new voice channel.");
            await newChannel.ModifyAsync(model =>
            {
                model.Name = name ?? $"{member.Username}'s channel";
                model.Userlimit = limit;
            });
            await newChannel.AddOverwriteAsync(member, Permissions.ManageChannels | Permissions.MuteMembers);

            member.PlaceInAsync(newChannel);
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                await ClearUnusedVoiceChannelAsync(newChannel);
            });
        }

        public async Task ClearUnusedVoiceChannelAsync(DiscordChannel channel)
        {
            if (!channel.Users.Any())
            {
                await channel.DeleteAsync();
            }
        }
    }
}
