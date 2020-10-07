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
    public class EditImage : BaseCommand
    {
        public const string ChatCommand = "editImage";
        // ;editImage <message-link> <new-image-url> <new-message>
        protected override bool CanBotExecute => false;

        public EditImage(ILogger<EditImage> logger, IPermissionHandler permissionHandler)
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

            // Message not found.
            if (oldMessage == null)
            {
                return ChatCommendExecutedResult.InternalError;
            }

            string imageUrl = message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[2];

            // Remove command and channel mention from message.
            // TODO: maybe remove command part to utils?
            const string pattern = @"^.\w+\s+[^\s]+\s+[^\s]+\s+";
            string text = message.Content;
            string editMessageText = Regex.Replace(text, pattern, "");

            await oldMessage.ModifyAsync(editMessageText.Trim() == "" ? oldMessage.Content : editMessageText,
                new DiscordEmbedBuilder {ImageUrl = imageUrl}.Build());

            return ChatCommendExecutedResult.Ok;
        }
    }
}
