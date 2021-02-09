using System;
using DSharpPlus.CommandsNext;

namespace HonzaBotner.Discord
{
    public class CommandConfigurator
    {
        public CommandConfigurator(Action<CommandsNextExtension> config) => Config = config;

        public Action<CommandsNextExtension> Config { get; }
    }
}
