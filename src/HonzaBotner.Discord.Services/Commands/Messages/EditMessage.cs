using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Discord.Services.Commands.Messages
{
    public class EditMessage : BaseCommand
    {
        public const string ChatCommand = "edit";
        // ;edit <message-link> <new-message>
        protected override bool CanBotExecute => false;

        protected override CommandPermission RequiredPermission => CommandPermission.Mod;

        public EditMessage(IPermissionHandler permissionHandler, ILogger<EditMessage> logger)
            : base(permissionHandler, logger)
        {
        }

        protected override async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client,
            DiscordMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Content.Split(" ").Length < 3) return ChatCommendExecutedResult.WrongSyntax;

            DiscordMessage? oldMessage = await
                DiscordHelper.FindMessageFromLink(message.Channel.Guild,
                    message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1]);

            // TODO: message not found.
            if (oldMessage == null)
            {
                return ChatCommendExecutedResult.InternalError;
            }

            const string pattern = @"^.\w+\s+[^\s]*\s+";
            string text = message.Content;
            string editMessageText = Regex.Replace(text, pattern, "");

            await oldMessage.ModifyAsync(editMessageText);

            return ChatCommendExecutedResult.Ok;
        }
    }
}
