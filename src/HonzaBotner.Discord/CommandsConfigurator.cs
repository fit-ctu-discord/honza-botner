using System;
using DSharpPlus.SlashCommands;

namespace HonzaBotner.Discord;

public class CommandsConfigurator
{
    public CommandsConfigurator(Action<SlashCommandsExtension> config) => Config = config;

    public Action<SlashCommandsExtension> Config { get; }
}
