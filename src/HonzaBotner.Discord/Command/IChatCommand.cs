using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Command
{
    public enum ChatCommendExecutedResult
    {
        Ok,
        WrongSyntax,
        InternalError,
        CannotBeUsedByBot,
    }

    public interface IChatCommand
    {
        Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken = default);
    }
}
