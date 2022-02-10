using DSharpPlus.Entities;

namespace HonzaBotner.Discord.Attributes;

public interface IOnlyForMemberCountAllowlistedAttribute
{
    DiscordEmbed GetFailedCheckDiscordEmbed()
    {
        return new DiscordEmbedBuilder()
            .WithTitle("Přístup zakázán")
            .WithDescription("Tento příkaz může používat pouze Moderátor a další allowlistnuté role.")
            .WithColor(DiscordColor.Violet)
            .Build();
    }
}
