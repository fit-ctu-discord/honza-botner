using System;
using DSharpPlus.SlashCommands;

namespace HonzaBotner.Discord
{
    public class SlashCommandsConfigurator
    {
        public SlashCommandsConfigurator(Action<SlashCommandsExtension> config) => Config = config;
        
        public Action<SlashCommandsExtension> Config { get; }
    }
}