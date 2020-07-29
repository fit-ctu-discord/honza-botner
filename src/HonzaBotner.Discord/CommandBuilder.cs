using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Discord
{
    public class CommandBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IList<(Type Type, string Command)> _commands;

        internal CommandBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            _commands = new List<(Type Type, string Command)>();
        }

        public CommandBuilder AddCommand<TCommand>(string commandText)
            where TCommand : IChatCommand
        {
            var commandType = typeof(TCommand);

            var command = (commandType, commandText);

            _commands.Add(command);
            _serviceCollection.AddScoped(commandType);

            return this;
        }

        internal CommandCollection ToCollection()
        {
            return new CommandCollection(_commands);
        }
    }
}
