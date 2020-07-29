using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OsBot.Core.Command
{
    public class CommandCollection
    {
        private readonly ImmutableDictionary<string, Type> _commands;
        public IEnumerable<string> AvailableCommands => _commands.Keys;

        internal CommandCollection(IEnumerable<(Type CommandType, string CommandText)> commands)
        {
            _commands = commands.ToImmutableDictionary(
                k => k.CommandText,
                v => v.CommandType
            );
        }

        public Type? GetCommandType(string command)
        {
            if (_commands.ContainsKey(command))
            {
                return _commands[command];
            }

            return null;
        }
    }
}