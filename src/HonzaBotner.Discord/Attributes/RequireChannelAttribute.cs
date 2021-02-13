using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace HonzaBotner.Discord.Services.Attributes
{
    public class RequireChannelAttribute : CheckBaseAttribute
    {
        public ulong ChannelId { get; private set; }

        public RequireChannelAttribute(ulong channelId)
        {
            ChannelId = channelId;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ChannelId == ctx.Channel.Id);
        }
    }
}

