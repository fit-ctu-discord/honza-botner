using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace HonzaBotner.Discord.Services
{
    public class VoiceManager: IVoiceManager
    {
        private readonly CommandConfigurator _configurator;
        private readonly DiscordWrapper _discordWrapper;
        private DiscordClient Client => _discordWrapper.Client;

        public VoiceManager(DiscordWrapper discordWrapper, CommandConfigurator configurator)
        {
            _discordWrapper = discordWrapper;
            _configurator = configurator;

            Console.WriteLine("hmmm");
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            Console.WriteLine("hmmm 2");

            //await Client.ConnectAsync();
            await Task.Delay(-1, cancellationToken);
        }

        public Task Client_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
        {
            Task.Run(async () =>
            {
                DiscordDmChannel channel = await args.Guild.Members[args.User.Id].CreateDmChannelAsync();
                await channel.SendMessageAsync(args.After.ToString());

                if (args.Guild.Id == 750055928669405258 && args.After.Channel.Id == 750055929340231716)
                {
                    IEnumerable<DiscordChannel> others = args.Guild.GetChannel(750055929340231714).Children
                        .Where(channel => channel.Id != 750055929340231716);
                    foreach (DiscordChannel discordChannel in others)
                    {
                        if (!discordChannel.Users.Any())
                        {
                            await discordChannel.DeleteAsync();
                        }
                    }

                    DiscordChannel cloned = await args.Channel.CloneAsync("Creates custom voice channel.");
                    await cloned.ModifyAsync(model => model.Name = "New channel from user " + args.User.Username);
                    await cloned.PlaceMemberAsync(await args.Guild.GetMemberAsync(args.User.Id));
                }
            });

            return Task.CompletedTask;
        }
    }
}
