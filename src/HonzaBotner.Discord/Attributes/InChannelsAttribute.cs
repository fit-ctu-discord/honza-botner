using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace HonzaBotner.Discord.Attributes
{
    public class InChannelsAttribute : CheckBaseAttribute
    {
        public ulong[] ChannelIds { get; private set; }

        public InChannelsAttribute(ulong[] channelIds)
        {
            ChannelIds = channelIds;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ChannelIds.Contains(ctx.Channel.Id));
        }
    }
}

