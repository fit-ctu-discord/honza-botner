using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers;

public class SlashManager : ISlashManager
{

    private readonly IGuildProvider _guildProvider;
    private readonly CommonCommandOptions _options;

    public SlashManager(IGuildProvider guildProvider, IOptions<CommonCommandOptions> options)
    {
        _guildProvider = guildProvider;
        _options = options.Value;
    }

    public async Task UpdateStartupPermissions()
    {
        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        DiscordRole modRole = guild.GetRole(_options.ModRoleId);
        var commands = await guild.GetApplicationCommandsAsync();
        foreach (var command in commands)
        {
            if (command.DefaultPermission ?? true) continue;
            var customPermissions = new List<DiscordApplicationCommandPermission> { new(modRole, true) };
            await guild.EditApplicationCommandPermissionsAsync(command, customPermissions);
        }
    }
}
