using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services.Options;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Discord.Services.Managers;

public class SlashManager : ISlashManager
{

    private readonly IGuildProvider _guildProvider;
    private readonly CommonCommandOptions _options;
    private readonly DiscordClient _client;

    public SlashManager(IGuildProvider guildProvider, IOptions<CommonCommandOptions> options, DiscordWrapper wrapper)
    {
        _guildProvider = guildProvider;
        _options = options.Value;
        _client = wrapper.Client;
    }

    public async Task UpdateStartupPermissions()
    {
        var newPermissions = new List<DiscordGuildApplicationCommandPermissions>();
        DiscordGuild guild = await _guildProvider.GetCurrentGuildAsync();
        DiscordRole modRole = guild.GetRole(_options.ModRoleId);
        var commands = _client.GetSlashCommands().RegisteredCommands;
        foreach (var command in commands.First(x => x.Key.Equals(_guildProvider.GuildId)).Value)
        {
            if (command.DefaultPermission ?? true) continue;
            var customPermissions = new List<DiscordApplicationCommandPermission> { new DiscordApplicationCommandPermission(modRole, true) };
            newPermissions.Add(new DiscordGuildApplicationCommandPermissions(command.Id, customPermissions));
        }

        await guild.BatchEditApplicationCommandPermissionsAsync(newPermissions);
    }

}
