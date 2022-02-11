using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

namespace HonzaBotner.Discord.Attributes;

public interface IRequireModAttribute
{
    /// <summary>
    /// Executes check on CommandContext whether the User is a Mod
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="help"></param>
    /// <returns></returns>
    Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help);
}
