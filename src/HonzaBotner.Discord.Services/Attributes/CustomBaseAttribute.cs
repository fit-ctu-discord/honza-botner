using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Attributes;

namespace HonzaBotner.Discord.Services.Attributes;

/// <summary>
/// Represents a base for all custom command pre-execution check attributes.
/// </summary>
public abstract class CustomBaseAttribute : CheckBaseAttribute, ICustomAttribute
{
    /// <summary>
    /// Builds a Discord embed with details of the fail.
    /// </summary>
    /// <returns>DiscordEmbed with details on why it did fail the pre-execution check.</returns>
    public abstract Task<DiscordEmbed> BuildFailedCheckDiscordEmbed();
}
