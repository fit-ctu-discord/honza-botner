using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using HonzaBotner.Discord.Command;

namespace HonzaBotner.Discord.Services.Commands.Messages
{
    public class EditMessage : IChatCommand
    {
        public const string ChatCommand = "edit";
        // ;edit <message-link> <new-message>

        public async Task<ChatCommendExecutedResult> ExecuteAsync(DiscordClient client, DiscordMessage message,
            CancellationToken cancellationToken)
        {
            if (message.Author.IsBot) return ChatCommendExecutedResult.CannotBeUsedByBot;
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

            try
            {
                await oldMessage.ModifyAsync(editMessageText);
            }
            catch
            {
                return ChatCommendExecutedResult.InternalError;
            }

            return ChatCommendExecutedResult.Ok;
        }
    }
}
