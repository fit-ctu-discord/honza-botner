using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace HonzaBotner.Discord
{
    public interface IReactionHandler
    {
        public enum Result
        {
            Continue,
            Stop
        }

        Task<Result> HandleAddAsync(MessageReactionAddEventArgs eventArgs) => Task.FromResult(Result.Continue);
        Task<Result> HandleRemoveAsync(MessageReactionRemoveEventArgs eventArgs) => Task.FromResult(Result.Continue);
    }
}
