using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.EventHandlers
{
    public class VoiceHandler : IEventHandler<VoiceStateUpdateEventArgs>
    {
        private readonly CustomVoiceOptions _voiceConfig;
        private readonly IVoiceManager _voiceManager;

        public VoiceHandler(IOptions<CustomVoiceOptions> options, IVoiceManager voiceManager)
        {
            _voiceConfig = options.Value;
            _voiceManager = voiceManager;
        }

        public async Task<EventHandlerResult> Handle(VoiceStateUpdateEventArgs args)
        {
            if (args.After.Channel?.Id == _voiceConfig.ClickChannelId)
            {
                await _voiceManager.AddNewVoiceChannelAsync(args.Channel,
                    await args.Guild.GetMemberAsync(args.User.Id));
            }

            if (args.Before?.Channel != null)
            {
                if (args.Before.Channel.Id != _voiceConfig.ClickChannelId)
                {
                    await _voiceManager.DeleteUnusedVoiceChannelAsync(args.Before.Channel);
                }
            }

            return EventHandlerResult.Continue;
        }
    }
}
