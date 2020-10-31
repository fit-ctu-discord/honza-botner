using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands.Polls
{
    public class PollCommand : BaseCommand
    {
        public const string ChatCommand = "poll";
        protected override CommandPermission RequiredPermission => CommandPermission.Authorized;
        protected override bool CanBotExecute => false;

        public PollCommand(IPermissionHandler permissionHandler, ILogger<PollCommand> logger)
            : base(permissionHandler, logger)
        {
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = ParseParameters(message.Content);
                var poll = CreatePoll(client, message.Author, parameters);

                await poll.Post(client, message.Channel);
                await message.DeleteAsync();

                return ChatCommendExecutedResult.Ok;
            }
            catch (ArgumentException ex)
            {
                await message.RespondAsync(ex.Message);
                return ChatCommendExecutedResult.WrongSyntax;
            }
        }

        protected IPoll CreatePoll(DiscordClient client, DiscordUser pollAuthor, List<string> parameters)
        {
            if (parameters.Count == 1)
            {
                return new YesNoPoll(pollAuthor.Username, pollAuthor.AvatarUrl, parameters[0]);
            }

            return new AbcPoll(pollAuthor.Username, pollAuthor.AvatarUrl, parameters[0], parameters.Skip(1).ToList());
        }

        protected List<string> ParseParameters(string message)
        {
            if (message.Count(c => c == '"') % 2 != 0)
            {
                throw new ArgumentException("Odd number of quotes");
            }

            // Pattern matches quoted string and captures the quoted content in a capture group
            // Matched string: `"Hello, world"`
            // Captured string: `Hello, world`
            const string quotedGroupPattern = @"""([^\""]*?)""";
            var matches = Regex.Matches(message, quotedGroupPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var parameters = matches
                .Where(m => m.Success && !string.IsNullOrEmpty(m.Value))
                .Select(m => m.Groups[1].Value).ToList();

            if (parameters.Count == 0)
            {
                throw new ArgumentException("No parameters matched");
            }

            return parameters;
        }
    }
}
